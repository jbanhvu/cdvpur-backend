using ChangdaeVinaPurchasingApi.Models.NaverWorks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using System.Globalization;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Services.NaverWorks;

public class NaverWorksSyncService
{
    private const string LeaveTypeComponentId = "a43c02d9-9dca-0fd7-6566-284140e31f71";
    private const string LeavePeriodComponentId = "a999e1d5-df77-fe62-0f2b-82a880f83d17";
    private const string LeaveDaysComponentId = "ff857af5-26f9-8f95-7bd5-9a8c196c07f5";
    private const string TimeUsedComponentId = "1ccae4ee-9ea0-b5eb-1f14-3eacf6a18546";
    private const string DeductHoursComponentId = "0d3cc565-077f-fe1c-0bd2-74be443fe28d";
    private const string ReasonComponentId = "b4f34a15-6fc8-81a1-5266-4d317cf5ad82";

    private readonly NaverWorksApprovalService _approvalService;
    private readonly IConfiguration _configuration;
    private readonly IOptions<NaverWorksOptions> _options;
    private readonly ILogger<NaverWorksSyncService> _logger;

    public NaverWorksSyncService(
        NaverWorksApprovalService approvalService,
        IConfiguration configuration,
        IOptions<NaverWorksOptions> options,
        ILogger<NaverWorksSyncService> logger)
    {
        _approvalService = approvalService;
        _configuration = configuration;
        _options = options;
        _logger = logger;
    }

    public async Task<NaverWorksSyncAllResult> SyncAsync(
        string type,
        DateTime fromDate,
        DateTime untilDate,
        CancellationToken cancellationToken = default)
    {
        string normalizedType = NormalizeSyncType(type);
        NaverWorksOptions options = _options.Value;
        Dictionary<string, NaverWorksSyncHandler> handlers = CreateSyncHandlers(options);

        List<NaverWorksSyncHandler> selectedHandlers = normalizedType == "all"
            ? handlers.Values.ToList()
            : handlers.TryGetValue(normalizedType, out NaverWorksSyncHandler? handler)
                ? [handler]
                : throw new InvalidOperationException($"Unsupported NAVER WORKS sync type: {type}");

        foreach (NaverWorksSyncHandler selectedHandler in selectedHandlers)
        {
            if (string.IsNullOrWhiteSpace(selectedHandler.DocumentFormId))
            {
                throw new InvalidOperationException($"NaverWorks:{selectedHandler.OptionName} is missing.");
            }
        }

        _logger.LogInformation(
            "Starting NAVER WORKS {SyncType} sync from {FromDate} to {UntilDate}.",
            normalizedType,
            fromDate,
            untilDate);

        List<NaverWorksApprovalDocument> documents = await _approvalService.GetApprovalDocumentsAsync(
            fromDate,
            untilDate,
            cancellationToken);

        Dictionary<string, NaverWorksSyncHandler> formHandlers = selectedHandlers
            .ToDictionary(
                item => item.DocumentFormId,
                item => item,
                StringComparer.OrdinalIgnoreCase);

        NaverWorksSyncAllResult result = new()
        {
            Type = normalizedType,
            TotalDocuments = documents.Count
        };

        foreach (NaverWorksSyncHandler selectedHandler in selectedHandlers)
        {
            result.Results[selectedHandler.Type] = new NaverWorksSyncTypeResult
            {
                Type = selectedHandler.Type
            };
        }

        foreach (NaverWorksApprovalDocument document in documents)
        {
            if (string.IsNullOrWhiteSpace(document.DocumentFormId) ||
                !formHandlers.TryGetValue(document.DocumentFormId, out NaverWorksSyncHandler? selectedHandler))
            {
                continue;
            }

            NaverWorksSyncTypeResult typeResult = result.Results[selectedHandler.Type];
            typeResult.Documents++;

            try
            {
                _logger.LogInformation(
                    "Syncing NAVER WORKS {SyncType} approvalDocumentId {ApprovalDocumentId}.",
                    selectedHandler.Type,
                    document.ApprovalDocumentId);

                JsonElement detailJson = await _approvalService.GetApprovalDocumentDetailJsonAsync(
                    document.ApprovalDocumentId,
                    cancellationToken);

                NaverWorksApprovalDetailResponse detail = _approvalService.ParseApprovalDetail(detailJson);
                string rawJson = detailJson.GetRawText();

                await using SqlConnection connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);

                await SaveRawAsync(connection, detail, rawJson, cancellationToken);
                await selectedHandler.SaveAsync(connection, detail, cancellationToken);

                typeResult.Synced++;
            }
            catch (Exception ex)
            {
                typeResult.Failed++;
                string error = $"{document.ApprovalDocumentId}: {ex.Message}";
                typeResult.Errors.Add(error);
                _logger.LogError(
                    ex,
                    "Failed to sync NAVER WORKS {SyncType} approvalDocumentId {ApprovalDocumentId}.",
                    selectedHandler.Type,
                    document.ApprovalDocumentId);
            }
        }

        result.MatchedDocuments = result.Results.Values.Sum(item => item.Documents);
        result.Synced = result.Results.Values.Sum(item => item.Synced);
        result.Failed = result.Results.Values.Sum(item => item.Failed);

        _logger.LogInformation(
            "Finished NAVER WORKS {SyncType} sync. Matched={MatchedDocuments}, Synced={Synced}, Failed={Failed}.",
            normalizedType,
            result.MatchedDocuments,
            result.Synced,
            result.Failed);

        return result;
    }

    public async Task<NaverWorksSyncResult> SyncLeaveAsync(
        DateTime fromDate,
        DateTime untilDate,
        CancellationToken cancellationToken = default)
    {
        string leaveDocumentFormId = _options.Value.LeaveDocumentFormId;
        if (string.IsNullOrWhiteSpace(leaveDocumentFormId))
        {
            throw new InvalidOperationException("NaverWorks:LeaveDocumentFormId is missing.");
        }

        _logger.LogInformation("Starting NAVER WORKS leave sync from {FromDate} to {UntilDate}.", fromDate, untilDate);

        List<NaverWorksApprovalDocument> documents = await _approvalService.GetApprovalDocumentsAsync(
            fromDate,
            untilDate,
            cancellationToken);

        List<NaverWorksApprovalDocument> leaveDocuments = documents
            .Where(document => string.Equals(document.DocumentFormId, leaveDocumentFormId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        NaverWorksSyncResult result = new()
        {
            TotalDocuments = documents.Count,
            LeaveDocuments = leaveDocuments.Count
        };

        _logger.LogInformation(
            "NAVER WORKS returned {TotalDocuments} documents, {LeaveDocuments} leave documents.",
            result.TotalDocuments,
            result.LeaveDocuments);

        foreach (NaverWorksApprovalDocument document in leaveDocuments)
        {
            try
            {
                _logger.LogInformation("Syncing NAVER WORKS approvalDocumentId {ApprovalDocumentId}.", document.ApprovalDocumentId);

                JsonElement detailJson = await _approvalService.GetApprovalDocumentDetailJsonAsync(
                    document.ApprovalDocumentId,
                    cancellationToken);

                NaverWorksApprovalDetailResponse detail = _approvalService.ParseApprovalDetail(detailJson);
                string rawJson = detailJson.GetRawText();
                ParsedLeaveBody parsedLeaveBody = ParseLeaveBody(detail.DocumentBody);

                await using SqlConnection connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);

                await SaveRawAsync(connection, detail, rawJson, cancellationToken);
                await SaveEmployeeLeaveAsync(connection, detail, parsedLeaveBody, cancellationToken);

                result.Synced++;
            }
            catch (Exception ex)
            {
                result.Failed++;
                string error = $"{document.ApprovalDocumentId}: {ex.Message}";
                result.Errors.Add(error);
                _logger.LogError(ex, "Failed to sync NAVER WORKS approvalDocumentId {ApprovalDocumentId}.", document.ApprovalDocumentId);
            }
        }

        _logger.LogInformation(
            "Finished NAVER WORKS leave sync. Synced={Synced}, Failed={Failed}.",
            result.Synced,
            result.Failed);

        return result;
    }

    public async Task<NaverWorksSyncResult> SyncPurchaseRequestAsync(
        DateTime fromDate,
        DateTime untilDate,
        CancellationToken cancellationToken = default)
    {
        NaverWorksOptions options = _options.Value;
        string documentFormId = options.PurchaseRequestDocumentFormId;
        if (string.IsNullOrWhiteSpace(documentFormId))
        {
            throw new InvalidOperationException("NaverWorks:PurchaseRequestDocumentFormId is missing.");
        }

        _logger.LogInformation("Starting NAVER WORKS purchase request sync from {FromDate} to {UntilDate}.", fromDate, untilDate);

        List<NaverWorksApprovalDocument> documents = await _approvalService.GetApprovalDocumentsAsync(
            fromDate,
            untilDate,
            cancellationToken);

        List<NaverWorksApprovalDocument> purchaseRequestDocuments = documents
            .Where(document => string.Equals(document.DocumentFormId, documentFormId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        NaverWorksSyncResult result = new()
        {
            TotalDocuments = documents.Count,
            PurchaseRequestDocuments = purchaseRequestDocuments.Count
        };

        foreach (NaverWorksApprovalDocument document in purchaseRequestDocuments)
        {
            try
            {
                _logger.LogInformation("Syncing NAVER WORKS purchase request approvalDocumentId {ApprovalDocumentId}.", document.ApprovalDocumentId);

                JsonElement detailJson = await _approvalService.GetApprovalDocumentDetailJsonAsync(
                    document.ApprovalDocumentId,
                    cancellationToken);

                NaverWorksApprovalDetailResponse detail = _approvalService.ParseApprovalDetail(detailJson);
                string rawJson = detailJson.GetRawText();
                ParsedPurchaseRequestBody purchaseRequest = ParsePurchaseRequestBody(detail, options);

                await using SqlConnection connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);

                await SaveRawAsync(connection, detail, rawJson, cancellationToken);
                int purchaseRequestId = await SavePurchaseRequestAsync(connection, detail, purchaseRequest, cancellationToken);

                if (purchaseRequest.Details.Rows.Count > 0)
                {
                    await SavePurchaseRequestDetailsAsync(connection, purchaseRequestId, purchaseRequest.Details, cancellationToken);
                }

                result.Synced++;
            }
            catch (Exception ex)
            {
                result.Failed++;
                string error = $"{document.ApprovalDocumentId}: {ex.Message}";
                result.Errors.Add(error);
                _logger.LogError(ex, "Failed to sync NAVER WORKS purchase request approvalDocumentId {ApprovalDocumentId}.", document.ApprovalDocumentId);
            }
        }

        _logger.LogInformation(
            "Finished NAVER WORKS purchase request sync. Synced={Synced}, Failed={Failed}.",
            result.Synced,
            result.Failed);

        return result;
    }

    public async Task<NaverWorksSyncResult> SyncBusinessTripAsync(
        DateTime fromDate,
        DateTime untilDate,
        CancellationToken cancellationToken = default)
    {
        NaverWorksOptions options = _options.Value;
        string documentFormId = options.BusinessTripDocumentFormId;
        if (string.IsNullOrWhiteSpace(documentFormId))
        {
            throw new InvalidOperationException("NaverWorks:BusinessTripDocumentFormId is missing.");
        }

        _logger.LogInformation("Starting NAVER WORKS business trip sync from {FromDate} to {UntilDate}.", fromDate, untilDate);

        List<NaverWorksApprovalDocument> documents = await _approvalService.GetApprovalDocumentsAsync(
            fromDate,
            untilDate,
            cancellationToken);

        List<NaverWorksApprovalDocument> businessTripDocuments = documents
            .Where(document => string.Equals(document.DocumentFormId, documentFormId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        NaverWorksSyncResult result = new()
        {
            TotalDocuments = documents.Count,
            BusinessTripDocuments = businessTripDocuments.Count
        };

        foreach (NaverWorksApprovalDocument document in businessTripDocuments)
        {
            try
            {
                _logger.LogInformation("Syncing NAVER WORKS business trip approvalDocumentId {ApprovalDocumentId}.", document.ApprovalDocumentId);

                JsonElement detailJson = await _approvalService.GetApprovalDocumentDetailJsonAsync(
                    document.ApprovalDocumentId,
                    cancellationToken);

                NaverWorksApprovalDetailResponse detail = _approvalService.ParseApprovalDetail(detailJson);
                string rawJson = detailJson.GetRawText();
                ParsedBusinessTripBody businessTrip = ParseBusinessTripBody(detail, options);

                await using SqlConnection connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);

                await SaveRawAsync(connection, detail, rawJson, cancellationToken);
                int businessTripId = await SaveBusinessTripAsync(connection, detail, businessTrip, cancellationToken);
                await SaveBusinessTripDetailsAsync(connection, businessTripId, businessTrip.Details, cancellationToken);

                result.Synced++;
            }
            catch (Exception ex)
            {
                result.Failed++;
                string error = $"{document.ApprovalDocumentId}: {ex.Message}";
                result.Errors.Add(error);
                _logger.LogError(ex, "Failed to sync NAVER WORKS business trip approvalDocumentId {ApprovalDocumentId}.", document.ApprovalDocumentId);
            }
        }

        _logger.LogInformation(
            "Finished NAVER WORKS business trip sync. Synced={Synced}, Failed={Failed}.",
            result.Synced,
            result.Failed);

        return result;
    }

    public async Task<NaverWorksSyncResult> SyncHiringAsync(
        DateTime fromDate,
        DateTime untilDate,
        CancellationToken cancellationToken = default)
    {
        NaverWorksOptions options = _options.Value;
        string documentFormId = options.HiringDocumentFormId;
        if (string.IsNullOrWhiteSpace(documentFormId))
        {
            throw new InvalidOperationException("NaverWorks:HiringDocumentFormId is missing.");
        }

        _logger.LogInformation("Starting NAVER WORKS hiring sync from {FromDate} to {UntilDate}.", fromDate, untilDate);

        List<NaverWorksApprovalDocument> documents = await _approvalService.GetApprovalDocumentsAsync(
            fromDate,
            untilDate,
            cancellationToken);

        List<NaverWorksApprovalDocument> hiringDocuments = documents
            .Where(document => string.Equals(document.DocumentFormId, documentFormId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        NaverWorksSyncResult result = new()
        {
            TotalDocuments = documents.Count,
            HiringDocuments = hiringDocuments.Count
        };

        foreach (NaverWorksApprovalDocument document in hiringDocuments)
        {
            try
            {
                _logger.LogInformation("Syncing NAVER WORKS hiring approvalDocumentId {ApprovalDocumentId}.", document.ApprovalDocumentId);

                JsonElement detailJson = await _approvalService.GetApprovalDocumentDetailJsonAsync(
                    document.ApprovalDocumentId,
                    cancellationToken);

                NaverWorksApprovalDetailResponse detail = _approvalService.ParseApprovalDetail(detailJson);
                string rawJson = detailJson.GetRawText();
                ParsedHiringBody hiring = ParseHiringBody(detail, options);

                await using SqlConnection connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);

                await SaveRawAsync(connection, detail, rawJson, cancellationToken);
                int hiringId = await SaveHiringAsync(connection, detail, cancellationToken);
                await SaveHiringDetailsAsync(connection, hiringId, hiring.Details, cancellationToken);

                result.Synced++;
            }
            catch (Exception ex)
            {
                result.Failed++;
                string error = $"{document.ApprovalDocumentId}: {ex.Message}";
                result.Errors.Add(error);
                _logger.LogError(ex, "Failed to sync NAVER WORKS hiring approvalDocumentId {ApprovalDocumentId}.", document.ApprovalDocumentId);
            }
        }

        _logger.LogInformation(
            "Finished NAVER WORKS hiring sync. Synced={Synced}, Failed={Failed}.",
            result.Synced,
            result.Failed);

        return result;
    }

    public async Task<NaverWorksSyncResult> SyncExitPermissionAsync(
        DateTime fromDate,
        DateTime untilDate,
        CancellationToken cancellationToken = default)
    {
        NaverWorksOptions options = _options.Value;
        string documentFormId = options.ExitPermissionDocumentFormId;
        if (string.IsNullOrWhiteSpace(documentFormId))
        {
            throw new InvalidOperationException("NaverWorks:ExitPermissionDocumentFormId is missing.");
        }

        _logger.LogInformation("Starting NAVER WORKS exit permission sync from {FromDate} to {UntilDate}.", fromDate, untilDate);

        List<NaverWorksApprovalDocument> documents = await _approvalService.GetApprovalDocumentsAsync(
            fromDate,
            untilDate,
            cancellationToken);

        List<NaverWorksApprovalDocument> exitPermissionDocuments = documents
            .Where(document => string.Equals(document.DocumentFormId, documentFormId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        NaverWorksSyncResult result = new()
        {
            TotalDocuments = documents.Count,
            ExitPermissionDocuments = exitPermissionDocuments.Count
        };

        foreach (NaverWorksApprovalDocument document in exitPermissionDocuments)
        {
            try
            {
                _logger.LogInformation("Syncing NAVER WORKS exit permission approvalDocumentId {ApprovalDocumentId}.", document.ApprovalDocumentId);

                JsonElement detailJson = await _approvalService.GetApprovalDocumentDetailJsonAsync(
                    document.ApprovalDocumentId,
                    cancellationToken);

                NaverWorksApprovalDetailResponse detail = _approvalService.ParseApprovalDetail(detailJson);
                string rawJson = detailJson.GetRawText();
                ParsedExitPermissionBody exitPermission = ParseExitPermissionBody(detail, options);

                await using SqlConnection connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);

                await SaveRawAsync(connection, detail, rawJson, cancellationToken);
                int exitPermissionId = await SaveExitPermissionAsync(connection, detail, exitPermission, cancellationToken);
                await SaveExitPermissionDetailsAsync(connection, exitPermissionId, exitPermission.Details, cancellationToken);

                result.Synced++;
            }
            catch (Exception ex)
            {
                result.Failed++;
                string error = $"{document.ApprovalDocumentId}: {ex.Message}";
                result.Errors.Add(error);
                _logger.LogError(ex, "Failed to sync NAVER WORKS exit permission approvalDocumentId {ApprovalDocumentId}.", document.ApprovalDocumentId);
            }
        }

        _logger.LogInformation(
            "Finished NAVER WORKS exit permission sync. Synced={Synced}, Failed={Failed}.",
            result.Synced,
            result.Failed);

        return result;
    }

    public async Task<NaverWorksSyncResult> SyncCompanyVisitorAsync(
        DateTime fromDate,
        DateTime untilDate,
        CancellationToken cancellationToken = default)
    {
        NaverWorksOptions options = _options.Value;
        string documentFormId = options.CompanyVisitorDocumentFormId;
        if (string.IsNullOrWhiteSpace(documentFormId))
        {
            throw new InvalidOperationException("NaverWorks:CompanyVisitorDocumentFormId is missing.");
        }

        _logger.LogInformation("Starting NAVER WORKS company visitor sync from {FromDate} to {UntilDate}.", fromDate, untilDate);

        List<NaverWorksApprovalDocument> documents = await _approvalService.GetApprovalDocumentsAsync(
            fromDate,
            untilDate,
            cancellationToken);

        List<NaverWorksApprovalDocument> companyVisitorDocuments = documents
            .Where(document => string.Equals(document.DocumentFormId, documentFormId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        NaverWorksSyncResult result = new()
        {
            TotalDocuments = documents.Count,
            CompanyVisitorDocuments = companyVisitorDocuments.Count
        };

        foreach (NaverWorksApprovalDocument document in companyVisitorDocuments)
        {
            try
            {
                _logger.LogInformation("Syncing NAVER WORKS company visitor approvalDocumentId {ApprovalDocumentId}.", document.ApprovalDocumentId);

                JsonElement detailJson = await _approvalService.GetApprovalDocumentDetailJsonAsync(
                    document.ApprovalDocumentId,
                    cancellationToken);

                NaverWorksApprovalDetailResponse detail = _approvalService.ParseApprovalDetail(detailJson);
                string rawJson = detailJson.GetRawText();
                ParsedCompanyVisitorBody companyVisitor = ParseCompanyVisitorBody(detail, options);

                await using SqlConnection connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);

                await SaveRawAsync(connection, detail, rawJson, cancellationToken);
                int companyVisitorId = await SaveCompanyVisitorAsync(connection, detail, companyVisitor, cancellationToken);
                await SaveCompanyVisitorDetailsAsync(connection, companyVisitorId, companyVisitor.Details, cancellationToken);

                result.Synced++;
            }
            catch (Exception ex)
            {
                result.Failed++;
                string error = $"{document.ApprovalDocumentId}: {ex.Message}";
                result.Errors.Add(error);
                _logger.LogError(ex, "Failed to sync NAVER WORKS company visitor approvalDocumentId {ApprovalDocumentId}.", document.ApprovalDocumentId);
            }
        }

        _logger.LogInformation(
            "Finished NAVER WORKS company visitor sync. Synced={Synced}, Failed={Failed}.",
            result.Synced,
            result.Failed);

        return result;
    }

    public async Task<NaverWorksSyncResult> SyncPurchaseAsync(
        DateTime fromDate,
        DateTime untilDate,
        CancellationToken cancellationToken = default)
    {
        NaverWorksOptions options = _options.Value;
        string documentFormId = options.PurchaseDocumentFormId;
        if (string.IsNullOrWhiteSpace(documentFormId))
        {
            throw new InvalidOperationException("NaverWorks:PurchaseDocumentFormId is missing.");
        }

        _logger.LogInformation("Starting NAVER WORKS purchase sync from {FromDate} to {UntilDate}.", fromDate, untilDate);

        List<NaverWorksApprovalDocument> documents = await _approvalService.GetApprovalDocumentsAsync(
            fromDate,
            untilDate,
            cancellationToken);

        List<NaverWorksApprovalDocument> purchaseDocuments = documents
            .Where(document => string.Equals(document.DocumentFormId, documentFormId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        NaverWorksSyncResult result = new()
        {
            TotalDocuments = documents.Count,
            PurchaseDocuments = purchaseDocuments.Count
        };

        foreach (NaverWorksApprovalDocument document in purchaseDocuments)
        {
            try
            {
                _logger.LogInformation("Syncing NAVER WORKS purchase approvalDocumentId {ApprovalDocumentId}.", document.ApprovalDocumentId);

                JsonElement detailJson = await _approvalService.GetApprovalDocumentDetailJsonAsync(
                    document.ApprovalDocumentId,
                    cancellationToken);

                NaverWorksApprovalDetailResponse detail = _approvalService.ParseApprovalDetail(detailJson);
                string rawJson = detailJson.GetRawText();
                ParsedPurchaseBody purchase = ParsePurchaseBody(detail, options);

                await using SqlConnection connection = CreateConnection();
                await connection.OpenAsync(cancellationToken);

                await SaveRawAsync(connection, detail, rawJson, cancellationToken);
                int purchaseId = await SavePurchaseAsync(connection, detail, purchase, cancellationToken);
                await SavePurchaseDetailsAsync(connection, purchaseId, purchase.Details, cancellationToken);
                await SavePurchaseRequestLinksAsync(connection, purchaseId, purchase.RequestLinks, cancellationToken);

                result.Synced++;
            }
            catch (Exception ex)
            {
                result.Failed++;
                string error = $"{document.ApprovalDocumentId}: {ex.Message}";
                result.Errors.Add(error);
                _logger.LogError(ex, "Failed to sync NAVER WORKS purchase approvalDocumentId {ApprovalDocumentId}.", document.ApprovalDocumentId);
            }
        }

        _logger.LogInformation(
            "Finished NAVER WORKS purchase sync. Synced={Synced}, Failed={Failed}.",
            result.Synced,
            result.Failed);

        return result;
    }

    private Dictionary<string, NaverWorksSyncHandler> CreateSyncHandlers(NaverWorksOptions options)
    {
        return new Dictionary<string, NaverWorksSyncHandler>(StringComparer.OrdinalIgnoreCase)
        {
            ["leave"] = new(
                "leave",
                options.LeaveDocumentFormId,
                nameof(options.LeaveDocumentFormId),
                async (connection, detail, cancellationToken) =>
                {
                    ParsedLeaveBody parsedLeaveBody = ParseLeaveBody(detail.DocumentBody);
                    await SaveEmployeeLeaveAsync(connection, detail, parsedLeaveBody, cancellationToken);
                }),
            ["purchase-request"] = new(
                "purchase-request",
                options.PurchaseRequestDocumentFormId,
                nameof(options.PurchaseRequestDocumentFormId),
                async (connection, detail, cancellationToken) =>
                {
                    ParsedPurchaseRequestBody purchaseRequest = ParsePurchaseRequestBody(detail, options);
                    int purchaseRequestId = await SavePurchaseRequestAsync(connection, detail, purchaseRequest, cancellationToken);
                    await SavePurchaseRequestDetailsAsync(connection, purchaseRequestId, purchaseRequest.Details, cancellationToken);
                }),
            ["purchase"] = new(
                "purchase",
                options.PurchaseDocumentFormId,
                nameof(options.PurchaseDocumentFormId),
                async (connection, detail, cancellationToken) =>
                {
                    ParsedPurchaseBody purchase = ParsePurchaseBody(detail, options);
                    int purchaseId = await SavePurchaseAsync(connection, detail, purchase, cancellationToken);
                    await SavePurchaseDetailsAsync(connection, purchaseId, purchase.Details, cancellationToken);
                    await SavePurchaseRequestLinksAsync(connection, purchaseId, purchase.RequestLinks, cancellationToken);
                }),
            ["business-trip"] = new(
                "business-trip",
                options.BusinessTripDocumentFormId,
                nameof(options.BusinessTripDocumentFormId),
                async (connection, detail, cancellationToken) =>
                {
                    ParsedBusinessTripBody businessTrip = ParseBusinessTripBody(detail, options);
                    int businessTripId = await SaveBusinessTripAsync(connection, detail, businessTrip, cancellationToken);
                    await SaveBusinessTripDetailsAsync(connection, businessTripId, businessTrip.Details, cancellationToken);
                }),
            ["hiring"] = new(
                "hiring",
                options.HiringDocumentFormId,
                nameof(options.HiringDocumentFormId),
                async (connection, detail, cancellationToken) =>
                {
                    ParsedHiringBody hiring = ParseHiringBody(detail, options);
                    int hiringId = await SaveHiringAsync(connection, detail, cancellationToken);
                    await SaveHiringDetailsAsync(connection, hiringId, hiring.Details, cancellationToken);
                }),
            ["exit-permission"] = new(
                "exit-permission",
                options.ExitPermissionDocumentFormId,
                nameof(options.ExitPermissionDocumentFormId),
                async (connection, detail, cancellationToken) =>
                {
                    ParsedExitPermissionBody exitPermission = ParseExitPermissionBody(detail, options);
                    int exitPermissionId = await SaveExitPermissionAsync(connection, detail, exitPermission, cancellationToken);
                    await SaveExitPermissionDetailsAsync(connection, exitPermissionId, exitPermission.Details, cancellationToken);
                }),
            ["company-visitor"] = new(
                "company-visitor",
                options.CompanyVisitorDocumentFormId,
                nameof(options.CompanyVisitorDocumentFormId),
                async (connection, detail, cancellationToken) =>
                {
                    ParsedCompanyVisitorBody companyVisitor = ParseCompanyVisitorBody(detail, options);
                    int companyVisitorId = await SaveCompanyVisitorAsync(connection, detail, companyVisitor, cancellationToken);
                    await SaveCompanyVisitorDetailsAsync(connection, companyVisitorId, companyVisitor.Details, cancellationToken);
                })
        };
    }

    private static string NormalizeSyncType(string type)
    {
        return type.Trim().ToLowerInvariant() switch
        {
            "all" => "all",
            "leave" => "leave",
            "purchase-request" or "purchase_request" or "purchaserequest" => "purchase-request",
            "purchase" => "purchase",
            "business-trip" or "business_trip" or "businesstrip" => "business-trip",
            "hiring" => "hiring",
            "exit-permission" or "exit_permission" or "exitpermission" => "exit-permission",
            "company-visitor" or "company_visitor" or "companyvisitor" or "visitor" => "company-visitor",
            _ => type.Trim().ToLowerInvariant()
        };
    }

    private static ParsedLeaveBody ParseLeaveBody(List<NaverWorksDocumentComponent> components)
    {
        NaverWorksDocumentComponent? leaveType = FindComponent(components, LeaveTypeComponentId);
        NaverWorksDocumentComponent? leavePeriod = FindComponent(components, LeavePeriodComponentId);
        NaverWorksDocumentComponent? leaveDays = FindComponent(components, LeaveDaysComponentId);
        NaverWorksDocumentComponent? timeUsed = FindComponent(components, TimeUsedComponentId);
        NaverWorksDocumentComponent? deductHours = FindComponent(components, DeductHoursComponentId);
        NaverWorksDocumentComponent? reason = FindComponent(components, ReasonComponentId);

        return new ParsedLeaveBody
        {
            LeaveType = ReadItemName(leaveType?.ComponentValue),
            FromDate = ReadNaverDate(leavePeriod?.ComponentValue, "startDate"),
            ToDate = ReadNaverDate(leavePeriod?.ComponentValue, "endDate"),
            LeaveDays = ReadDecimal(leaveDays?.ComponentValue, "value"),
            TimeUsed = ReadString(timeUsed?.ComponentValue, "value"),
            DeductHours = ReadDecimal(deductHours?.ComponentValue, "value"),
            Reason = ReadString(reason?.ComponentValue, "value")
        };
    }

    private static ParsedPurchaseRequestBody ParsePurchaseRequestBody(
        NaverWorksApprovalDetailResponse detail,
        NaverWorksOptions options)
    {
        DateTime requestDate = detail.CreatedTime?.DateTime.Date ?? DateTime.Today;
        DateTime? requiredDate = ReadComponentDate(detail.DocumentBody, options.PurchaseRequestRequiredDateComponentId);
        string currencyCode = ReadComponentString(detail.DocumentBody, options.PurchaseRequestCurrencyCodeComponentId) ??
            options.PurchaseRequestDefaultCurrencyCode;

        ParsedPurchaseRequestBody result = new()
        {
            RequestDate = requestDate,
            RequiredDate = requiredDate,
            CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? "VND" : currencyCode,
            PurchaseType = ReadComponentString(detail.DocumentBody, options.PurchaseRequestPurchaseTypeComponentId),
            IsUrgent = ReadComponentBool(detail.DocumentBody, options.PurchaseRequestIsUrgentComponentId) ?? false,
            Reason = ReadComponentString(detail.DocumentBody, options.PurchaseRequestReasonComponentId),
            RecommendedSupplier = ReadComponentString(detail.DocumentBody, options.PurchaseRequestRecommendedSupplierComponentId),
            Details = CreatePurchaseRequestDetailsTable()
        };

        FillPurchaseRequestDetails(result.Details, detail.DocumentBody, options);
        result.TotalAmount = result.Details.Rows
            .Cast<DataRow>()
            .Sum(row => row["EstimatedAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["EstimatedAmount"], CultureInfo.InvariantCulture));

        return result;
    }

    private static ParsedPurchaseBody ParsePurchaseBody(
        NaverWorksApprovalDetailResponse detail,
        NaverWorksOptions options)
    {
        string currencyCode = ReadComponentString(detail.DocumentBody, options.PurchaseCurrencyCodeComponentId) ??
            ReadComponentCurrencyCode(detail.DocumentBody, options.PurchaseTotalAmountComponentId) ??
            "VND";

        ParsedPurchaseBody result = new()
        {
            PurchaseDate = detail.CreatedTime?.DateTime.Date ?? DateTime.Today,
            PurchaseOverview = ReadComponentString(detail.DocumentBody, options.PurchaseOverviewComponentId),
            SelectedSupplierReason = ReadComponentString(detail.DocumentBody, options.PurchaseSelectedSupplierReasonComponentId),
            CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? "VND" : currencyCode,
            TotalAmount = ReadComponentCurrencyValue(detail.DocumentBody, options.PurchaseTotalAmountComponentId) ?? 0,
            VatType = ReadComponentString(detail.DocumentBody, options.PurchaseVatTypeComponentId),
            PaymentTerms = ReadComponentString(detail.DocumentBody, options.PurchasePaymentTermsComponentId),
            Details = [],
            RequestLinks = []
        };

        FillPurchaseDetails(result.Details, detail.DocumentBody, options);
        FillPurchaseRequestLinks(result.RequestLinks, detail.DocumentBody, options);

        if (result.TotalAmount == 0 && result.Details.Count > 0)
        {
            result.TotalAmount = result.Details.Sum(item => item.Amount ?? 0);
        }

        return result;
    }

    private static ParsedBusinessTripBody ParseBusinessTripBody(
        NaverWorksApprovalDetailResponse detail,
        NaverWorksOptions options)
    {
        NaverWorksDocumentComponent? period = FindComponent(detail.DocumentBody, options.BusinessTripPeriodComponentId);

        ParsedBusinessTripBody result = new()
        {
            TripType = ReadComponentString(detail.DocumentBody, options.BusinessTripTypeComponentId),
            Destination = ReadComponentString(detail.DocumentBody, options.BusinessTripDestinationComponentId),
            FromDate = ReadNaverDate(period?.ComponentValue, "startDate"),
            ToDate = ReadNaverDate(period?.ComponentValue, "endDate"),
            Purpose = ReadComponentString(detail.DocumentBody, options.BusinessTripPurposeComponentId),
            Companions = ReadComponentUsers(detail.DocumentBody, options.BusinessTripCompanionsComponentId),
            PassengerCount = ReadComponentString(detail.DocumentBody, options.BusinessTripPassengerCountComponentId),
            UseTime = ReadComponentString(detail.DocumentBody, options.BusinessTripUseTimeComponentId),
            UseCorporateCard = ReadComponentBool(detail.DocumentBody, options.BusinessTripUseCorporateCardComponentId) ?? false,
            Details = []
        };

        FillBusinessTripDetails(result.Details, detail.DocumentBody, options);
        result.TotalAmount = result.Details.Sum(item => item.TotalAmount ?? 0);

        return result;
    }

    private static ParsedHiringBody ParseHiringBody(
        NaverWorksApprovalDetailResponse detail,
        NaverWorksOptions options)
    {
        List<OrgUnitValue> placementDepartments = ReadComponentOrgUnits(
            detail.DocumentBody,
            options.HiringPlacementDepartmentComponentId);

        if (placementDepartments.Count == 0)
        {
            placementDepartments.Add(new OrgUnitValue
            {
                OrgUnitId = null,
                OrgUnitName = null
            });
        }

        string? jobTitle = ReadComponentString(detail.DocumentBody, options.HiringJobTitleComponentId);
        string? positionLevel = ReadComponentString(detail.DocumentBody, options.HiringPositionLevelComponentId);
        int? headCount = ReadComponentInt(detail.DocumentBody, options.HiringHeadCountComponentId);
        string? hiringReason = ReadComponentString(detail.DocumentBody, options.HiringReasonComponentId);
        string? reasonDetail = ReadComponentString(detail.DocumentBody, options.HiringReasonDetailComponentId);
        DateTime? desiredStartDate = ReadComponentDate(detail.DocumentBody, options.HiringDesiredStartDateComponentId);
        string? salaryLevel = ReadComponentString(detail.DocumentBody, options.HiringSalaryLevelComponentId);
        string? qualification = ReadComponentString(detail.DocumentBody, options.HiringQualificationComponentId);

        ParsedHiringBody result = new();
        foreach (OrgUnitValue department in placementDepartments)
        {
            result.Details.Add(new ParsedHiringDetailBody
            {
                PlacementDepartmentId = department.OrgUnitId,
                PlacementDepartmentName = department.OrgUnitName,
                JobTitle = jobTitle,
                PositionLevel = positionLevel,
                HeadCount = headCount,
                HiringReason = hiringReason,
                ReasonDetail = reasonDetail,
                DesiredStartDate = desiredStartDate,
                SalaryLevel = salaryLevel,
                Qualification = qualification
            });
        }

        return result;
    }

    private static List<OrgUnitValue> ReadComponentOrgUnits(List<NaverWorksDocumentComponent> components, string componentId)
    {
        if (string.IsNullOrWhiteSpace(componentId))
        {
            return [];
        }

        NaverWorksDocumentComponent? component = FindComponent(components, componentId);
        if (component is null ||
            !TryGetProperty(component.ComponentValue, "orgUnits", out JsonElement orgUnits) ||
            orgUnits.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        List<OrgUnitValue> result = [];
        foreach (JsonElement orgUnit in orgUnits.EnumerateArray())
        {
            result.Add(new OrgUnitValue
            {
                OrgUnitId = ReadString(orgUnit, "orgUnitId"),
                OrgUnitName = ReadString(orgUnit, "orgUnitName")
            });
        }

        return result;
    }

    private static int? ReadComponentInt(List<NaverWorksDocumentComponent> components, string componentId)
    {
        string? text = ReadComponentString(components, componentId);
        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
            ? value
            : null;
    }

    private static string? ReadComponentUsers(List<NaverWorksDocumentComponent> components, string componentId)
    {
        if (string.IsNullOrWhiteSpace(componentId))
        {
            return null;
        }

        NaverWorksDocumentComponent? component = FindComponent(components, componentId);
        if (component is null ||
            !TryGetProperty(component.ComponentValue, "users", out JsonElement users) ||
            users.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        List<string> names = [];
        foreach (JsonElement user in users.EnumerateArray())
        {
            string? name = ReadString(user, "name") ??
                ReadString(user, "userName") ??
                ReadString(user, "displayName") ??
                ReadString(user, "email") ??
                ReadFlexibleString(user);

            if (!string.IsNullOrWhiteSpace(name))
            {
                names.Add(name);
            }
        }

        return names.Count == 0 ? null : string.Join(", ", names);
    }

    private static void FillBusinessTripDetails(
        List<ParsedBusinessTripDetailBody> details,
        List<NaverWorksDocumentComponent> components,
        NaverWorksOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BusinessTripExpenseComponentId))
        {
            return;
        }

        NaverWorksDocumentComponent? component = FindComponent(components, options.BusinessTripExpenseComponentId);
        if (component is null ||
            !TryGetProperty(component.ComponentValue, "table", out JsonElement tableValue) ||
            !TryGetProperty(tableValue, "headers", out JsonElement headers) ||
            !TryGetProperty(tableValue, "rowDatas", out JsonElement rowDatas) ||
            headers.ValueKind != JsonValueKind.Array ||
            rowDatas.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        Dictionary<string, int> columnIndexes = [];
        int index = 0;
        foreach (JsonElement header in headers.EnumerateArray())
        {
            AddColumnIndex(columnIndexes, ReadString(header, "cellId"), index);
            AddColumnIndex(columnIndexes, ReadString(header, "cellName"), index);
            index++;
        }

        foreach (JsonElement row in rowDatas.EnumerateArray())
        {
            if (TryGetProperty(row, "hasSubTotalRow", out JsonElement hasSubTotalRow) &&
                hasSubTotalRow.ValueKind is JsonValueKind.True)
            {
                continue;
            }

            if (!TryGetProperty(row, "cellDatas", out JsonElement cellDatas) ||
                cellDatas.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            ParsedBusinessTripDetailBody detail = new()
            {
                TransportationAmount = ReadTableCellDecimal(cellDatas, columnIndexes, options.BusinessTripExpenseTransportationKey),
                LodgingAmount = ReadTableCellDecimal(cellDatas, columnIndexes, options.BusinessTripExpenseLodgingKey),
                DailyAllowanceAmount = ReadTableCellDecimal(cellDatas, columnIndexes, options.BusinessTripExpenseDailyAllowanceKey),
                OtherAmount = ReadTableCellDecimal(cellDatas, columnIndexes, options.BusinessTripExpenseOtherKey),
                TotalAmount = ReadTableCellDecimal(cellDatas, columnIndexes, options.BusinessTripExpenseTotalKey)
            };

            if (detail.TotalAmount is null)
            {
                detail.TotalAmount = (detail.TransportationAmount ?? 0) +
                    (detail.LodgingAmount ?? 0) +
                    (detail.DailyAllowanceAmount ?? 0) +
                    (detail.OtherAmount ?? 0);
            }

            details.Add(detail);
        }
    }

    private static void FillExitPermissionDetails(
        List<ParsedExitPermissionDetailBody> details,
        List<NaverWorksDocumentComponent> components,
        NaverWorksOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ExitPermissionDetailComponentId))
        {
            return;
        }

        NaverWorksDocumentComponent? component = FindComponent(components, options.ExitPermissionDetailComponentId);
        if (component is null ||
            !TryGetProperty(component.ComponentValue, "table", out JsonElement tableValue) ||
            !TryGetProperty(tableValue, "headers", out JsonElement headers) ||
            !TryGetProperty(tableValue, "rowDatas", out JsonElement rowDatas) ||
            headers.ValueKind != JsonValueKind.Array ||
            rowDatas.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        Dictionary<string, int> columnIndexes = [];
        int index = 0;
        foreach (JsonElement header in headers.EnumerateArray())
        {
            AddColumnIndex(columnIndexes, ReadString(header, "cellId"), index);
            AddColumnIndex(columnIndexes, ReadString(header, "cellName"), index);
            index++;
        }

        foreach (JsonElement row in rowDatas.EnumerateArray())
        {
            if (!TryGetProperty(row, "cellDatas", out JsonElement cellDatas) ||
                cellDatas.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            string? itemName = ReadTableCellString(cellDatas, columnIndexes, options.ExitPermissionDetailItemNameKey);
            string? itemCode = ReadTableCellString(cellDatas, columnIndexes, options.ExitPermissionDetailItemCodeKey);
            string? remark = ReadTableCellString(cellDatas, columnIndexes, options.ExitPermissionDetailRemarkKey);

            if (string.IsNullOrWhiteSpace(itemName) &&
                string.IsNullOrWhiteSpace(itemCode) &&
                string.IsNullOrWhiteSpace(remark))
            {
                continue;
            }

            details.Add(new ParsedExitPermissionDetailBody
            {
                LineNo = ReadTableCellInt(cellDatas, columnIndexes, options.ExitPermissionDetailLineNoKey),
                ItemName = itemName,
                ItemCode = itemCode,
                Unit = ReadTableCellString(cellDatas, columnIndexes, options.ExitPermissionDetailUnitKey),
                Qty = ReadTableCellDecimal(cellDatas, columnIndexes, options.ExitPermissionDetailQtyKey),
                Remark = remark
            });
        }
    }

    private static void FillCompanyVisitorDetails(
        List<ParsedCompanyVisitorDetailBody> details,
        List<NaverWorksDocumentComponent> components,
        NaverWorksOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.CompanyVisitorDetailComponentId))
        {
            return;
        }

        NaverWorksDocumentComponent? component = FindComponent(components, options.CompanyVisitorDetailComponentId);
        if (component is null ||
            !TryGetProperty(component.ComponentValue, "table", out JsonElement tableValue) ||
            !TryGetProperty(tableValue, "headers", out JsonElement headers) ||
            !TryGetProperty(tableValue, "rowDatas", out JsonElement rowDatas) ||
            headers.ValueKind != JsonValueKind.Array ||
            rowDatas.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        Dictionary<string, int> columnIndexes = [];
        int index = 0;
        foreach (JsonElement header in headers.EnumerateArray())
        {
            AddColumnIndex(columnIndexes, ReadString(header, "cellId"), index);
            AddColumnIndex(columnIndexes, ReadString(header, "cellName"), index);
            index++;
        }

        foreach (JsonElement row in rowDatas.EnumerateArray())
        {
            if (!TryGetProperty(row, "cellDatas", out JsonElement cellDatas) ||
                cellDatas.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            string? visitorName = ReadTableCellString(cellDatas, columnIndexes, options.CompanyVisitorDetailNameKey);
            string? visitorTitle = ReadTableCellString(cellDatas, columnIndexes, options.CompanyVisitorDetailTitleKey);
            string? identityNo = ReadTableCellString(cellDatas, columnIndexes, options.CompanyVisitorDetailIdentityNoKey);

            if (string.IsNullOrWhiteSpace(visitorName) &&
                string.IsNullOrWhiteSpace(visitorTitle) &&
                string.IsNullOrWhiteSpace(identityNo))
            {
                continue;
            }

            details.Add(new ParsedCompanyVisitorDetailBody
            {
                VisitorName = visitorName,
                VisitorTitle = visitorTitle,
                IdentityNo = identityNo
            });
        }
    }

    private static int? ReadTableCellInt(
        JsonElement cellDatas,
        Dictionary<string, int> columnIndexes,
        string key)
    {
        string? text = ReadTableCellString(cellDatas, columnIndexes, key);
        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number)
            ? number
            : null;
    }

    private static string? ReadComponentCurrencyCode(List<NaverWorksDocumentComponent> components, string componentId)
    {
        if (string.IsNullOrWhiteSpace(componentId))
        {
            return null;
        }

        NaverWorksDocumentComponent? component = FindComponent(components, componentId);
        return component is null ? null : ReadString(component.ComponentValue, "currencyCode");
    }

    private static decimal? ReadComponentCurrencyValue(List<NaverWorksDocumentComponent> components, string componentId)
    {
        if (string.IsNullOrWhiteSpace(componentId))
        {
            return null;
        }

        NaverWorksDocumentComponent? component = FindComponent(components, componentId);
        string? text = component is null ? null : ReadString(component.ComponentValue, "currencyValue");
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value)
            ? value
            : null;
    }

    private static string? ReadComponentString(List<NaverWorksDocumentComponent> components, string componentId)
    {
        if (string.IsNullOrWhiteSpace(componentId))
        {
            return null;
        }

        NaverWorksDocumentComponent? component = FindComponent(components, componentId);
        return component is null ? null : ReadFlexibleString(component.ComponentValue);
    }

    private static DateTime? ReadComponentDate(List<NaverWorksDocumentComponent> components, string componentId)
    {
        string? text = ReadComponentString(components, componentId);
        if (DateTime.TryParseExact(text, "yyyy.MM.dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime naverDate))
        {
            return naverDate.Date;
        }

        return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)
            ? date.Date
            : null;
    }

    private static bool? ReadComponentBool(List<NaverWorksDocumentComponent> components, string componentId)
    {
        string? text = ReadComponentString(components, componentId);
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (bool.TryParse(text, out bool boolean))
        {
            return boolean;
        }

        return text.Trim() is "1" or "Y" or "y" or "Yes" or "YES" or "true" or "TRUE";
    }

    private static string? ReadFlexibleString(JsonElement element)
    {
        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            return element.GetString();
        }

        if (element.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
        {
            return element.ToString();
        }

        foreach (string name in new[] { "value", "text", "displayValue", "itemName", "name" })
        {
            string? value = ReadString(element, name);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        string? date = ReadString(element, "date");
        if (!string.IsNullOrWhiteSpace(date))
        {
            return date;
        }

        return ReadItemName(element) ?? element.ToString();
    }

    private static ParsedExitPermissionBody ParseExitPermissionBody(
        NaverWorksApprovalDetailResponse detail,
        NaverWorksOptions options)
    {
        UserValue? carrier = ReadComponentFirstUser(detail.DocumentBody, options.ExitPermissionCarrierComponentId);
        OrgUnitValue? carrierDepartment = ReadComponentOrgUnits(
            detail.DocumentBody,
            options.ExitPermissionCarrierDepartmentComponentId)
            .FirstOrDefault();

        ParsedExitPermissionBody result = new()
        {
            ExitType = ReadComponentString(detail.DocumentBody, options.ExitPermissionTypeComponentId),
            ExpectedReturnDate = ReadComponentDate(detail.DocumentBody, options.ExitPermissionExpectedReturnDateComponentId),
            CarrierUserId = carrier?.UserId,
            CarrierUserName = carrier?.UserName,
            CarrierDepartmentId = carrierDepartment?.OrgUnitId,
            CarrierDepartmentName = carrierDepartment?.OrgUnitName,
            ReceiverCompany = ReadComponentString(detail.DocumentBody, options.ExitPermissionReceiverCompanyComponentId),
            VehicleNo = ReadComponentString(detail.DocumentBody, options.ExitPermissionVehicleNoComponentId),
            DriverName = ReadComponentString(detail.DocumentBody, options.ExitPermissionDriverNameComponentId),
            Reason = ReadComponentString(detail.DocumentBody, options.ExitPermissionReasonComponentId),
            Details = []
        };

        FillExitPermissionDetails(result.Details, detail.DocumentBody, options);

        return result;
    }

    private static UserValue? ReadComponentFirstUser(List<NaverWorksDocumentComponent> components, string componentId)
    {
        if (string.IsNullOrWhiteSpace(componentId))
        {
            return null;
        }

        NaverWorksDocumentComponent? component = FindComponent(components, componentId);
        if (component is null ||
            !TryGetProperty(component.ComponentValue, "users", out JsonElement users) ||
            users.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        JsonElement user = users.EnumerateArray().FirstOrDefault();
        if (user.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        return new UserValue
        {
            UserId = ReadString(user, "userId"),
            UserName = ReadString(user, "userName") ??
                ReadString(user, "name") ??
                ReadString(user, "displayName")
        };
    }

    private static ParsedCompanyVisitorBody ParseCompanyVisitorBody(
        NaverWorksApprovalDetailResponse detail,
        NaverWorksOptions options)
    {
        NaverWorksDocumentComponent? time = FindComponent(detail.DocumentBody, options.CompanyVisitorTimeComponentId);
        UserValue? contact = ReadComponentFirstUser(detail.DocumentBody, options.CompanyVisitorContactComponentId);

        ParsedCompanyVisitorBody result = new()
        {
            CompanyName = ReadComponentString(detail.DocumentBody, options.CompanyVisitorCompanyComponentId),
            RepresentativeName = ReadComponentString(detail.DocumentBody, options.CompanyVisitorRepresentativeComponentId),
            VisitDate = ReadComponentDate(detail.DocumentBody, options.CompanyVisitorDateComponentId),
            StartTime = ReadNaverTime(time?.ComponentValue, "startTime"),
            EndTime = ReadNaverTime(time?.ComponentValue, "endTime"),
            Purpose = ReadComponentString(detail.DocumentBody, options.CompanyVisitorPurposeComponentId),
            ContactUserId = contact?.UserId,
            ContactUserName = contact?.UserName,
            Details = []
        };

        FillCompanyVisitorDetails(result.Details, detail.DocumentBody, options);

        return result;
    }

    private static DataTable CreatePurchaseRequestDetailsTable()
    {
        DataTable table = new();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("UseDepartment", typeof(string));
        table.Columns.Add("ItemName", typeof(string));
        table.Columns.Add("Specification", typeof(string));
        table.Columns.Add("Unit", typeof(string));
        table.Columns.Add("Quantity", typeof(decimal));
        table.Columns.Add("EstimatedUnitPrice", typeof(decimal));
        table.Columns.Add("EstimatedAmount", typeof(decimal));

        return table;
    }

    private static void FillPurchaseRequestDetails(
        DataTable table,
        List<NaverWorksDocumentComponent> components,
        NaverWorksOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.PurchaseRequestDetailComponentId))
        {
            return;
        }

        NaverWorksDocumentComponent? detailComponent = FindComponent(components, options.PurchaseRequestDetailComponentId);
        if (detailComponent is null)
        {
            return;
        }

        if (TryFillPurchaseRequestTableDetails(table, detailComponent.ComponentValue, options))
        {
            return;
        }

        foreach (JsonElement row in ReadRows(detailComponent.ComponentValue))
        {
            string? itemName = ReadRowString(row, options.PurchaseRequestDetailItemNameKey);
            if (string.IsNullOrWhiteSpace(itemName))
            {
                continue;
            }

            decimal quantity = ReadRowDecimal(row, options.PurchaseRequestDetailQuantityKey) ?? 0;
            decimal? estimatedUnitPrice = ReadRowDecimal(row, options.PurchaseRequestDetailEstimatedUnitPriceKey);
            decimal? estimatedAmount = ReadRowDecimal(row, options.PurchaseRequestDetailEstimatedAmountKey) ??
                (estimatedUnitPrice.HasValue ? quantity * estimatedUnitPrice.Value : null);

            table.Rows.Add(
                0,
                ReadRowString(row, options.PurchaseRequestDetailUseDepartmentKey) ?? (object)DBNull.Value,
                itemName,
                ReadRowString(row, options.PurchaseRequestDetailSpecificationKey) ?? (object)DBNull.Value,
                ReadRowString(row, options.PurchaseRequestDetailUnitKey) ?? (object)DBNull.Value,
                quantity,
                estimatedUnitPrice ?? (object)DBNull.Value,
                estimatedAmount ?? (object)DBNull.Value);
        }
    }

    private static bool TryFillPurchaseRequestTableDetails(
        DataTable table,
        JsonElement componentValue,
        NaverWorksOptions options)
    {
        if (!TryGetProperty(componentValue, "table", out JsonElement tableValue) ||
            !TryGetProperty(tableValue, "headers", out JsonElement headers) ||
            !TryGetProperty(tableValue, "rowDatas", out JsonElement rowDatas) ||
            headers.ValueKind != JsonValueKind.Array ||
            rowDatas.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        Dictionary<string, int> columnIndexes = [];
        int index = 0;
        foreach (JsonElement header in headers.EnumerateArray())
        {
            AddColumnIndex(columnIndexes, ReadString(header, "cellId"), index);
            AddColumnIndex(columnIndexes, ReadString(header, "cellName"), index);
            index++;
        }

        foreach (JsonElement row in rowDatas.EnumerateArray())
        {
            if (TryGetProperty(row, "hasSubTotalRow", out JsonElement hasSubTotalRow) &&
                hasSubTotalRow.ValueKind is JsonValueKind.True)
            {
                continue;
            }

            if (!TryGetProperty(row, "cellDatas", out JsonElement cellDatas) ||
                cellDatas.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            string? itemName = ReadTableCellString(cellDatas, columnIndexes, options.PurchaseRequestDetailItemNameKey);
            if (string.IsNullOrWhiteSpace(itemName))
            {
                continue;
            }

            decimal quantity = ReadTableCellDecimal(cellDatas, columnIndexes, options.PurchaseRequestDetailQuantityKey) ?? 0;
            decimal? estimatedUnitPrice = ReadTableCellDecimal(cellDatas, columnIndexes, options.PurchaseRequestDetailEstimatedUnitPriceKey);
            decimal? estimatedAmount = ReadTableCellDecimal(cellDatas, columnIndexes, options.PurchaseRequestDetailEstimatedAmountKey) ??
                (estimatedUnitPrice.HasValue ? quantity * estimatedUnitPrice.Value : null);

            table.Rows.Add(
                0,
                ReadTableCellString(cellDatas, columnIndexes, options.PurchaseRequestDetailUseDepartmentKey) ?? (object)DBNull.Value,
                itemName,
                ReadTableCellString(cellDatas, columnIndexes, options.PurchaseRequestDetailSpecificationKey) ?? (object)DBNull.Value,
                ReadTableCellString(cellDatas, columnIndexes, options.PurchaseRequestDetailUnitKey) ?? (object)DBNull.Value,
                quantity,
                estimatedUnitPrice ?? (object)DBNull.Value,
                estimatedAmount ?? (object)DBNull.Value);
        }

        return true;
    }

    private static void FillPurchaseDetails(
        List<ParsedPurchaseDetailBody> details,
        List<NaverWorksDocumentComponent> components,
        NaverWorksOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.PurchaseComparisonTableComponentId))
        {
            return;
        }

        NaverWorksDocumentComponent? component = FindComponent(components, options.PurchaseComparisonTableComponentId);
        if (component is null ||
            !TryGetProperty(component.ComponentValue, "table", out JsonElement tableValue) ||
            !TryGetProperty(tableValue, "headers", out JsonElement headers) ||
            !TryGetProperty(tableValue, "rowDatas", out JsonElement rowDatas) ||
            headers.ValueKind != JsonValueKind.Array ||
            rowDatas.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        Dictionary<string, int> columnIndexes = [];
        int index = 0;
        foreach (JsonElement header in headers.EnumerateArray())
        {
            AddColumnIndex(columnIndexes, ReadString(header, "cellId"), index);
            AddColumnIndex(columnIndexes, ReadString(header, "cellName"), index);
            index++;
        }

        foreach (JsonElement row in rowDatas.EnumerateArray())
        {
            if (!TryGetProperty(row, "cellDatas", out JsonElement cellDatas) ||
                cellDatas.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            string? itemName = ReadTableCellString(cellDatas, columnIndexes, options.PurchaseDetailItemNameKey);
            if (string.IsNullOrWhiteSpace(itemName))
            {
                continue;
            }

            details.Add(new ParsedPurchaseDetailBody
            {
                ItemName = itemName,
                SupplierName = ReadTableCellString(cellDatas, columnIndexes, options.PurchaseDetailSupplierNameKey),
                Amount = ReadTableCellDecimal(cellDatas, columnIndexes, options.PurchaseDetailAmountKey),
                Delivery = ReadTableCellString(cellDatas, columnIndexes, options.PurchaseDetailDeliveryKey),
                Note = ReadTableCellString(cellDatas, columnIndexes, options.PurchaseDetailNoteKey)
            });
        }
    }

    private static void FillPurchaseRequestLinks(
        List<ParsedPurchaseRequestLinkBody> links,
        List<NaverWorksDocumentComponent> components,
        NaverWorksOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.PurchaseRelatedDocumentsComponentId))
        {
            return;
        }

        NaverWorksDocumentComponent? component = FindComponent(components, options.PurchaseRelatedDocumentsComponentId);
        if (component is null ||
            !TryGetProperty(component.ComponentValue, "relatedDocuments", out JsonElement relatedDocuments) ||
            relatedDocuments.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (JsonElement relatedDocument in relatedDocuments.EnumerateArray())
        {
            links.Add(new ParsedPurchaseRequestLinkBody
            {
                ApprovalDocumentId = ReadLong(relatedDocument, "approvalDocumentId").ToString(CultureInfo.InvariantCulture),
                DocumentNumber = ReadString(relatedDocument, "documentNumber"),
                Title = ReadString(relatedDocument, "title")
            });
        }
    }

    private static void AddColumnIndex(Dictionary<string, int> columnIndexes, string? key, int index)
    {
        if (!string.IsNullOrWhiteSpace(key))
        {
            columnIndexes[key] = index;
        }
    }

    private static string? ReadTableCellString(
        JsonElement cellDatas,
        Dictionary<string, int> columnIndexes,
        string key)
    {
        if (string.IsNullOrWhiteSpace(key) ||
            !columnIndexes.TryGetValue(key, out int index) ||
            index < 0 ||
            index >= cellDatas.GetArrayLength())
        {
            return null;
        }

        JsonElement cell = cellDatas[index];
        return TryGetProperty(cell, "value", out JsonElement value)
            ? ReadFlexibleString(value)
            : ReadFlexibleString(cell);
    }

    private static decimal? ReadTableCellDecimal(
        JsonElement cellDatas,
        Dictionary<string, int> columnIndexes,
        string key)
    {
        string? text = ReadTableCellString(cellDatas, columnIndexes, key);
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal number)
            ? number
            : null;
    }

    private static IEnumerable<JsonElement> ReadRows(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Array)
        {
            return value.EnumerateArray().Select(item => item.Clone()).ToList();
        }

        if (value.ValueKind != JsonValueKind.Object)
        {
            return [];
        }

        foreach (string name in new[] { "rows", "items", "values", "value", "data" })
        {
            if (TryGetProperty(value, name, out JsonElement rows) && rows.ValueKind == JsonValueKind.Array)
            {
                return rows.EnumerateArray().Select(item => item.Clone()).ToList();
            }
        }

        return [];
    }

    private static string? ReadRowString(JsonElement row, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        if (TryGetProperty(row, key, out JsonElement direct))
        {
            return ReadFlexibleString(direct);
        }

        JsonElement cells = default;
        if ((!TryGetProperty(row, "cells", out cells) && !TryGetProperty(row, "columns", out cells)) ||
            cells.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (JsonElement cell in cells.EnumerateArray())
        {
            string? cellKey = ReadString(cell, "key") ??
                ReadString(cell, "name") ??
                ReadString(cell, "title") ??
                ReadString(cell, "componentId") ??
                ReadString(cell, "columnId");

            if (!string.Equals(cellKey, key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (TryGetProperty(cell, "value", out JsonElement value))
            {
                return ReadFlexibleString(value);
            }

            return ReadFlexibleString(cell);
        }

        return null;
    }

    private static decimal? ReadRowDecimal(JsonElement row, string key)
    {
        string? text = ReadRowString(row, key);
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal number)
            ? number
            : null;
    }

    private static NaverWorksDocumentComponent? FindComponent(
        IEnumerable<NaverWorksDocumentComponent> components,
        string componentId)
    {
        return components.FirstOrDefault(component =>
            string.Equals(component.ComponentId, componentId, StringComparison.OrdinalIgnoreCase));
    }

    private static string? ReadItemName(JsonElement? value)
    {
        if (value is null ||
            !TryGetProperty(value.Value, "items", out JsonElement items) ||
            items.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        JsonElement first = items.EnumerateArray().FirstOrDefault();
        return first.ValueKind == JsonValueKind.Undefined ? null : ReadString(first, "itemName");
    }

    private static DateTime? ReadNaverDate(JsonElement? value, string propertyName)
    {
        string? text = value is null ? null : ReadString(value.Value, propertyName);
        return DateTime.TryParseExact(
            text,
            "yyyy.MM.dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime date)
            ? date.Date
            : null;
    }

    private static TimeSpan? ReadNaverTime(JsonElement? value, string propertyName)
    {
        string? text = value is null ? null : ReadString(value.Value, propertyName);
        return TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out TimeSpan time)
            ? time
            : null;
    }

    private static decimal? ReadDecimal(JsonElement? value, string propertyName)
    {
        string? text = value is null ? null : ReadString(value.Value, propertyName);
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal number)
            ? number
            : null;
    }

    private static string? ReadString(JsonElement? element, string propertyName)
    {
        if (element is null)
        {
            return null;
        }

        return ReadString(element.Value, propertyName);
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out JsonElement value) ||
            value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private async Task SaveRawAsync(
        SqlConnection connection,
        NaverWorksApprovalDetailResponse detail,
        string rawJson,
        CancellationToken cancellationToken)
    {
        await using SqlCommand command = new("CDV_NaverWorksApprovalRaw_Upsert", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(SqlBigInt("@ApprovalDocumentId", detail.ApprovalDocumentId));
        command.Parameters.Add(SqlNVarChar("@DocumentFormId", 100, detail.DocumentFormId));
        command.Parameters.Add(SqlNVarChar("@DocumentNumber", 100, detail.DocumentNumber));
        command.Parameters.Add(SqlNVarChar("@UserId", 100, detail.UserId));
        command.Parameters.Add(SqlNVarChar("@UserName", 200, detail.UserName));
        command.Parameters.Add(SqlNVarChar("@OrgUnitId", 100, detail.OrgUnitId));
        command.Parameters.Add(SqlNVarChar("@OrgUnitName", 200, detail.OrgUnitName));
        command.Parameters.Add(SqlNVarChar("@Title", 500, detail.Title));
        command.Parameters.Add(SqlNVarChar("@Status", 50, detail.Status));
        command.Parameters.Add(SqlDateTimeOffset("@CreatedTime", detail.CreatedTime));
        command.Parameters.Add(SqlDateTimeOffset("@CompletedTime", detail.CompletedTime));
        command.Parameters.Add(SqlNVarChar("@RawJson", -1, rawJson));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task SaveEmployeeLeaveAsync(
        SqlConnection connection,
        NaverWorksApprovalDetailResponse detail,
        ParsedLeaveBody leave,
        CancellationToken cancellationToken)
    {
        await using SqlCommand command = new("CDV_EmployeeLeave_Upsert", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(SqlBigInt("@ApprovalDocumentId", detail.ApprovalDocumentId));
        command.Parameters.Add(SqlNVarChar("@DocumentNumber", 100, detail.DocumentNumber));
        command.Parameters.Add(SqlNVarChar("@DocumentFormId", 100, detail.DocumentFormId));
        command.Parameters.Add(SqlNVarChar("@NaverUserId", 100, detail.UserId));
        command.Parameters.Add(SqlNVarChar("@EmployeeId", 50, null));
        command.Parameters.Add(SqlNVarChar("@EmployeeName", 200, detail.UserName));
        command.Parameters.Add(SqlNVarChar("@OrgUnitId", 100, detail.OrgUnitId));
        command.Parameters.Add(SqlNVarChar("@DepartmentName", 200, detail.OrgUnitName));
        command.Parameters.Add(SqlNVarChar("@LeaveType", 100, leave.LeaveType));
        command.Parameters.Add(SqlDate("@FromDate", leave.FromDate));
        command.Parameters.Add(SqlDate("@ToDate", leave.ToDate));
        command.Parameters.Add(SqlDecimal("@LeaveDays", leave.LeaveDays));
        command.Parameters.Add(SqlNVarChar("@TimeUsed", 100, leave.TimeUsed));
        command.Parameters.Add(SqlDecimal("@DeductHours", leave.DeductHours));
        command.Parameters.Add(SqlNVarChar("@Reason", 1000, leave.Reason));
        command.Parameters.Add(SqlNVarChar("@ApprovalStatus", 50, detail.Status));
        command.Parameters.Add(SqlDateTimeOffset("@CreatedTime", detail.CreatedTime));
        command.Parameters.Add(SqlDateTimeOffset("@CompletedTime", detail.CompletedTime));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<int> SaveBusinessTripAsync(
        SqlConnection connection,
        NaverWorksApprovalDetailResponse detail,
        ParsedBusinessTripBody businessTrip,
        CancellationToken cancellationToken)
    {
        string tripNo = string.IsNullOrWhiteSpace(detail.DocumentNumber)
            ? $"NW-{detail.ApprovalDocumentId}"
            : detail.DocumentNumber!;

        await using SqlCommand command = new("CDV_BusinessTrip_Upsert", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(SqlVarChar("@TripNo", 50, tripNo));
        command.Parameters.Add(SqlNVarChar("@Title", 1000, detail.Title));
        command.Parameters.Add(SqlNVarChar("@RequesterUserId", 100, detail.UserId));
        command.Parameters.Add(SqlNVarChar("@RequesterUserName", 400, detail.UserName));
        command.Parameters.Add(SqlNVarChar("@DepartmentId", 100, detail.OrgUnitId));
        command.Parameters.Add(SqlNVarChar("@DepartmentName", 400, detail.OrgUnitName));
        command.Parameters.Add(SqlNVarChar("@TripType", 100, businessTrip.TripType));
        command.Parameters.Add(SqlNVarChar("@Destination", 500, businessTrip.Destination));
        command.Parameters.Add(SqlDate("@FromDate", businessTrip.FromDate));
        command.Parameters.Add(SqlDate("@ToDate", businessTrip.ToDate));
        command.Parameters.Add(SqlNVarChar("@Purpose", -1, businessTrip.Purpose));
        command.Parameters.Add(SqlNVarChar("@Companions", -1, businessTrip.Companions));
        command.Parameters.Add(SqlNVarChar("@PassengerCount", 100, businessTrip.PassengerCount));
        command.Parameters.Add(SqlNVarChar("@UseTime", 100, businessTrip.UseTime));
        command.Parameters.Add(new SqlParameter("@UseCorporateCard", SqlDbType.Bit) { Value = businessTrip.UseCorporateCard });
        command.Parameters.Add(SqlDecimal("@TotalAmount", businessTrip.TotalAmount));
        command.Parameters.Add(SqlVarChar("@Status", 30, detail.Status));
        command.Parameters.Add(SqlVarChar("@ApprovalDocumentId", 100, detail.ApprovalDocumentId.ToString(CultureInfo.InvariantCulture)));
        command.Parameters.Add(SqlVarChar("@DocumentFormId", 100, detail.DocumentFormId));
        command.Parameters.Add(SqlDateTimeOffset("@CreatedTime", detail.CreatedTime));
        command.Parameters.Add(SqlDateTimeOffset("@CompletedTime", detail.CompletedTime));

        object? value = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    private static async Task SaveBusinessTripDetailsAsync(
        SqlConnection connection,
        int businessTripId,
        List<ParsedBusinessTripDetailBody> details,
        CancellationToken cancellationToken)
    {
        await using SqlCommand deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = "DELETE FROM [nhvpa3en_vpa01].[CDV_BusinessTripDetail] WHERE BusinessTripId = @BusinessTripId;";
        deleteCommand.Parameters.Add(SqlInt("@BusinessTripId", businessTripId));
        await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

        foreach (ParsedBusinessTripDetailBody detail in details)
        {
            await using SqlCommand insertCommand = connection.CreateCommand();
            insertCommand.CommandText = """
                INSERT INTO [nhvpa3en_vpa01].[CDV_BusinessTripDetail]
                (
                    BusinessTripId,
                    TransportationAmount,
                    LodgingAmount,
                    DailyAllowanceAmount,
                    OtherAmount,
                    TotalAmount
                )
                VALUES
                (
                    @BusinessTripId,
                    @TransportationAmount,
                    @LodgingAmount,
                    @DailyAllowanceAmount,
                    @OtherAmount,
                    @TotalAmount
                );
                """;

            insertCommand.Parameters.Add(SqlInt("@BusinessTripId", businessTripId));
            insertCommand.Parameters.Add(SqlDecimal("@TransportationAmount", detail.TransportationAmount));
            insertCommand.Parameters.Add(SqlDecimal("@LodgingAmount", detail.LodgingAmount));
            insertCommand.Parameters.Add(SqlDecimal("@DailyAllowanceAmount", detail.DailyAllowanceAmount));
            insertCommand.Parameters.Add(SqlDecimal("@OtherAmount", detail.OtherAmount));
            insertCommand.Parameters.Add(SqlDecimal("@TotalAmount", detail.TotalAmount));

            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task<int> SaveHiringAsync(
        SqlConnection connection,
        NaverWorksApprovalDetailResponse detail,
        CancellationToken cancellationToken)
    {
        string hiringNo = string.IsNullOrWhiteSpace(detail.DocumentNumber)
            ? $"NW-{detail.ApprovalDocumentId}"
            : detail.DocumentNumber!;

        await using SqlCommand command = new("CDV_Hiring_Upsert", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(SqlVarChar("@HiringNo", 50, hiringNo));
        command.Parameters.Add(SqlNVarChar("@Title", 1000, detail.Title));
        command.Parameters.Add(SqlNVarChar("@RequesterUserId", 100, detail.UserId));
        command.Parameters.Add(SqlNVarChar("@RequesterUserName", 400, detail.UserName));
        command.Parameters.Add(SqlNVarChar("@RequesterDepartmentId", 100, detail.OrgUnitId));
        command.Parameters.Add(SqlNVarChar("@RequesterDepartmentName", 400, detail.OrgUnitName));
        command.Parameters.Add(SqlVarChar("@Status", 30, detail.Status));
        command.Parameters.Add(SqlVarChar("@ApprovalDocumentId", 100, detail.ApprovalDocumentId.ToString(CultureInfo.InvariantCulture)));
        command.Parameters.Add(SqlVarChar("@DocumentFormId", 100, detail.DocumentFormId));
        command.Parameters.Add(SqlDateTimeOffset("@CreatedTime", detail.CreatedTime));
        command.Parameters.Add(SqlDateTimeOffset("@CompletedTime", detail.CompletedTime));

        object? value = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    private static async Task SaveHiringDetailsAsync(
        SqlConnection connection,
        int hiringId,
        List<ParsedHiringDetailBody> details,
        CancellationToken cancellationToken)
    {
        await using SqlCommand deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = "DELETE FROM [nhvpa3en_vpa01].[CDV_HiringDetail] WHERE HiringId = @HiringId;";
        deleteCommand.Parameters.Add(SqlInt("@HiringId", hiringId));
        await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

        foreach (ParsedHiringDetailBody detail in details)
        {
            await using SqlCommand insertCommand = connection.CreateCommand();
            insertCommand.CommandText = """
                INSERT INTO [nhvpa3en_vpa01].[CDV_HiringDetail]
                (
                    HiringId,
                    PlacementDepartmentId,
                    PlacementDepartmentName,
                    JobTitle,
                    PositionLevel,
                    HeadCount,
                    HiringReason,
                    ReasonDetail,
                    DesiredStartDate,
                    SalaryLevel,
                    Qualification
                )
                VALUES
                (
                    @HiringId,
                    @PlacementDepartmentId,
                    @PlacementDepartmentName,
                    @JobTitle,
                    @PositionLevel,
                    @HeadCount,
                    @HiringReason,
                    @ReasonDetail,
                    @DesiredStartDate,
                    @SalaryLevel,
                    @Qualification
                );
                """;

            insertCommand.Parameters.Add(SqlInt("@HiringId", hiringId));
            insertCommand.Parameters.Add(SqlNVarChar("@PlacementDepartmentId", 100, detail.PlacementDepartmentId));
            insertCommand.Parameters.Add(SqlNVarChar("@PlacementDepartmentName", 400, detail.PlacementDepartmentName));
            insertCommand.Parameters.Add(SqlNVarChar("@JobTitle", 500, detail.JobTitle));
            insertCommand.Parameters.Add(SqlNVarChar("@PositionLevel", 200, detail.PositionLevel));
            insertCommand.Parameters.Add(SqlInt("@HeadCount", detail.HeadCount));
            insertCommand.Parameters.Add(SqlNVarChar("@HiringReason", 200, detail.HiringReason));
            insertCommand.Parameters.Add(SqlNVarChar("@ReasonDetail", 1000, detail.ReasonDetail));
            insertCommand.Parameters.Add(SqlDate("@DesiredStartDate", detail.DesiredStartDate));
            insertCommand.Parameters.Add(SqlNVarChar("@SalaryLevel", 200, detail.SalaryLevel));
            insertCommand.Parameters.Add(SqlNVarChar("@Qualification", -1, detail.Qualification));

            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task<int> SaveExitPermissionAsync(
        SqlConnection connection,
        NaverWorksApprovalDetailResponse detail,
        ParsedExitPermissionBody exitPermission,
        CancellationToken cancellationToken)
    {
        string exitNo = string.IsNullOrWhiteSpace(detail.DocumentNumber)
            ? $"NW-{detail.ApprovalDocumentId}"
            : detail.DocumentNumber!;

        await using SqlCommand command = new("CDV_ExitPermission_Upsert", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(SqlVarChar("@ExitNo", 50, exitNo));
        command.Parameters.Add(SqlNVarChar("@Title", 1000, detail.Title));
        command.Parameters.Add(SqlNVarChar("@RequesterUserId", 100, detail.UserId));
        command.Parameters.Add(SqlNVarChar("@RequesterUserName", 400, detail.UserName));
        command.Parameters.Add(SqlNVarChar("@RequesterDepartmentId", 100, detail.OrgUnitId));
        command.Parameters.Add(SqlNVarChar("@RequesterDepartmentName", 400, detail.OrgUnitName));
        command.Parameters.Add(SqlNVarChar("@ExitType", 200, exitPermission.ExitType));
        command.Parameters.Add(SqlDate("@ExpectedReturnDate", exitPermission.ExpectedReturnDate));
        command.Parameters.Add(SqlNVarChar("@CarrierUserId", 100, exitPermission.CarrierUserId));
        command.Parameters.Add(SqlNVarChar("@CarrierUserName", 400, exitPermission.CarrierUserName));
        command.Parameters.Add(SqlNVarChar("@CarrierDepartmentId", 100, exitPermission.CarrierDepartmentId));
        command.Parameters.Add(SqlNVarChar("@CarrierDepartmentName", 400, exitPermission.CarrierDepartmentName));
        command.Parameters.Add(SqlNVarChar("@ReceiverCompany", 500, exitPermission.ReceiverCompany));
        command.Parameters.Add(SqlNVarChar("@VehicleNo", 100, exitPermission.VehicleNo));
        command.Parameters.Add(SqlNVarChar("@DriverName", 200, exitPermission.DriverName));
        command.Parameters.Add(SqlNVarChar("@Reason", -1, exitPermission.Reason));
        command.Parameters.Add(SqlVarChar("@Status", 30, detail.Status));
        command.Parameters.Add(SqlVarChar("@ApprovalDocumentId", 100, detail.ApprovalDocumentId.ToString(CultureInfo.InvariantCulture)));
        command.Parameters.Add(SqlVarChar("@DocumentFormId", 100, detail.DocumentFormId));
        command.Parameters.Add(SqlDateTimeOffset("@CreatedTime", detail.CreatedTime));
        command.Parameters.Add(SqlDateTimeOffset("@CompletedTime", detail.CompletedTime));

        object? value = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    private static async Task SaveExitPermissionDetailsAsync(
        SqlConnection connection,
        int exitPermissionId,
        List<ParsedExitPermissionDetailBody> details,
        CancellationToken cancellationToken)
    {
        await using SqlCommand deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = "DELETE FROM [nhvpa3en_vpa01].[CDV_ExitPermissionDetail] WHERE ExitPermissionId = @ExitPermissionId;";
        deleteCommand.Parameters.Add(SqlInt("@ExitPermissionId", exitPermissionId));
        await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

        foreach (ParsedExitPermissionDetailBody detail in details)
        {
            await using SqlCommand insertCommand = connection.CreateCommand();
            insertCommand.CommandText = """
                INSERT INTO [nhvpa3en_vpa01].[CDV_ExitPermissionDetail]
                (
                    ExitPermissionId,
                    [LineNo],
                    ItemName,
                    ItemCode,
                    Unit,
                    Qty,
                    Remark
                )
                VALUES
                (
                    @ExitPermissionId,
                    @LineNo,
                    @ItemName,
                    @ItemCode,
                    @Unit,
                    @Qty,
                    @Remark
                );
                """;

            insertCommand.Parameters.Add(SqlInt("@ExitPermissionId", exitPermissionId));
            insertCommand.Parameters.Add(SqlInt("@LineNo", detail.LineNo));
            insertCommand.Parameters.Add(SqlNVarChar("@ItemName", 500, detail.ItemName));
            insertCommand.Parameters.Add(SqlNVarChar("@ItemCode", 200, detail.ItemCode));
            insertCommand.Parameters.Add(SqlNVarChar("@Unit", 100, detail.Unit));
            insertCommand.Parameters.Add(SqlDecimal("@Qty", detail.Qty));
            insertCommand.Parameters.Add(SqlNVarChar("@Remark", 1000, detail.Remark));

            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task<int> SaveCompanyVisitorAsync(
        SqlConnection connection,
        NaverWorksApprovalDetailResponse detail,
        ParsedCompanyVisitorBody companyVisitor,
        CancellationToken cancellationToken)
    {
        string visitorNo = string.IsNullOrWhiteSpace(detail.DocumentNumber)
            ? $"NW-{detail.ApprovalDocumentId}"
            : detail.DocumentNumber!;

        await using SqlCommand command = new("CDV_CompanyVisitor_Upsert", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(SqlVarChar("@VisitorNo", 50, visitorNo));
        command.Parameters.Add(SqlNVarChar("@Title", 1000, detail.Title));
        command.Parameters.Add(SqlNVarChar("@RequesterUserId", 100, detail.UserId));
        command.Parameters.Add(SqlNVarChar("@RequesterUserName", 400, detail.UserName));
        command.Parameters.Add(SqlNVarChar("@RequesterDepartmentId", 100, detail.OrgUnitId));
        command.Parameters.Add(SqlNVarChar("@RequesterDepartmentName", 400, detail.OrgUnitName));
        command.Parameters.Add(SqlNVarChar("@CompanyName", 500, companyVisitor.CompanyName));
        command.Parameters.Add(SqlNVarChar("@RepresentativeName", 400, companyVisitor.RepresentativeName));
        command.Parameters.Add(SqlDate("@VisitDate", companyVisitor.VisitDate));
        command.Parameters.Add(SqlTime("@StartTime", companyVisitor.StartTime));
        command.Parameters.Add(SqlTime("@EndTime", companyVisitor.EndTime));
        command.Parameters.Add(SqlNVarChar("@Purpose", -1, companyVisitor.Purpose));
        command.Parameters.Add(SqlNVarChar("@ContactUserId", 100, companyVisitor.ContactUserId));
        command.Parameters.Add(SqlNVarChar("@ContactUserName", 400, companyVisitor.ContactUserName));
        command.Parameters.Add(SqlVarChar("@Status", 30, detail.Status));
        command.Parameters.Add(SqlVarChar("@ApprovalDocumentId", 100, detail.ApprovalDocumentId.ToString(CultureInfo.InvariantCulture)));
        command.Parameters.Add(SqlVarChar("@DocumentFormId", 100, detail.DocumentFormId));
        command.Parameters.Add(SqlDateTimeOffset("@CreatedTime", detail.CreatedTime));
        command.Parameters.Add(SqlDateTimeOffset("@CompletedTime", detail.CompletedTime));

        object? value = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    private static async Task SaveCompanyVisitorDetailsAsync(
        SqlConnection connection,
        int companyVisitorId,
        List<ParsedCompanyVisitorDetailBody> details,
        CancellationToken cancellationToken)
    {
        await using SqlCommand deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = "DELETE FROM [nhvpa3en_vpa01].[CDV_CompanyVisitorDetail] WHERE CompanyVisitorId = @CompanyVisitorId;";
        deleteCommand.Parameters.Add(SqlInt("@CompanyVisitorId", companyVisitorId));
        await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

        foreach (ParsedCompanyVisitorDetailBody detail in details)
        {
            await using SqlCommand insertCommand = connection.CreateCommand();
            insertCommand.CommandText = """
                INSERT INTO [nhvpa3en_vpa01].[CDV_CompanyVisitorDetail]
                (
                    CompanyVisitorId,
                    VisitorName,
                    VisitorTitle,
                    IdentityNo
                )
                VALUES
                (
                    @CompanyVisitorId,
                    @VisitorName,
                    @VisitorTitle,
                    @IdentityNo
                );
                """;

            insertCommand.Parameters.Add(SqlInt("@CompanyVisitorId", companyVisitorId));
            insertCommand.Parameters.Add(SqlNVarChar("@VisitorName", 400, detail.VisitorName));
            insertCommand.Parameters.Add(SqlNVarChar("@VisitorTitle", 400, detail.VisitorTitle));
            insertCommand.Parameters.Add(SqlNVarChar("@IdentityNo", 200, detail.IdentityNo));

            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task<int> SavePurchaseRequestAsync(
        SqlConnection connection,
        NaverWorksApprovalDetailResponse detail,
        ParsedPurchaseRequestBody purchaseRequest,
        CancellationToken cancellationToken)
    {
        string prNo = string.IsNullOrWhiteSpace(detail.DocumentNumber)
            ? $"NW-{detail.ApprovalDocumentId}"
            : detail.DocumentNumber!;

        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = """
            IF EXISTS
            (
                SELECT 1
                FROM [nhvpa3en_vpa01].[CDV_PurchaseRequest]
                WHERE ApprovalDocumentId = @ApprovalDocumentId
            )
            BEGIN
                UPDATE [nhvpa3en_vpa01].[CDV_PurchaseRequest]
                SET
                    PRNo = @PRNo,
                    Title = @Title,
                    RequesterUserId = @RequesterUserId,
                    RequesterUserName = @RequesterUserName,
                    DepartmentId = @DepartmentId,
                    DepartmentName = @DepartmentName,
                    RequestDate = @RequestDate,
                    RequiredDate = @RequiredDate,
                    CurrencyCode = @CurrencyCode,
                    PurchaseType = @PurchaseType,
                    IsUrgent = @IsUrgent,
                    Reason = @Reason,
                    RecommendedSupplier = @RecommendedSupplier,
                    TotalAmount = @TotalAmount,
                    Status = @Status,
                    UpdatedBy = @UpdatedBy,
                    UpdatedDate = SYSDATETIME()
                WHERE ApprovalDocumentId = @ApprovalDocumentId;
            END
            ELSE
            BEGIN
                INSERT INTO [nhvpa3en_vpa01].[CDV_PurchaseRequest]
                (
                    PRNo,
                    Title,
                    RequesterUserId,
                    RequesterUserName,
                    DepartmentId,
                    DepartmentName,
                    RequestDate,
                    RequiredDate,
                    CurrencyCode,
                    PurchaseType,
                    IsUrgent,
                    Reason,
                    RecommendedSupplier,
                    TotalAmount,
                    Status,
                    ApprovalDocumentId,
                    CreatedBy,
                    CreatedDate
                )
                VALUES
                (
                    @PRNo,
                    @Title,
                    @RequesterUserId,
                    @RequesterUserName,
                    @DepartmentId,
                    @DepartmentName,
                    @RequestDate,
                    @RequiredDate,
                    @CurrencyCode,
                    @PurchaseType,
                    @IsUrgent,
                    @Reason,
                    @RecommendedSupplier,
                    @TotalAmount,
                    @Status,
                    @ApprovalDocumentId,
                    @CreatedBy,
                    SYSDATETIME()
                );
            END

            SELECT Id
            FROM [nhvpa3en_vpa01].[CDV_PurchaseRequest]
            WHERE ApprovalDocumentId = @ApprovalDocumentId;
            """;

        command.Parameters.Add(SqlNVarChar("@ApprovalDocumentId", 100, detail.ApprovalDocumentId.ToString(CultureInfo.InvariantCulture)));
        command.Parameters.Add(SqlVarChar("@PRNo", 50, prNo));
        command.Parameters.Add(SqlNVarChar("@Title", 1000, detail.Title));
        command.Parameters.Add(SqlInt("@RequesterUserId", ParseInt(detail.UserId)));
        command.Parameters.Add(SqlNVarChar("@RequesterUserName", 400, detail.UserName));
        command.Parameters.Add(SqlInt("@DepartmentId", null));
        command.Parameters.Add(SqlNVarChar("@DepartmentName", 400, detail.OrgUnitName));
        command.Parameters.Add(SqlDate("@RequestDate", purchaseRequest.RequestDate));
        command.Parameters.Add(SqlDate("@RequiredDate", purchaseRequest.RequiredDate));
        command.Parameters.Add(SqlVarChar("@CurrencyCode", 10, purchaseRequest.CurrencyCode));
        command.Parameters.Add(SqlVarChar("@PurchaseType", 50, purchaseRequest.PurchaseType));
        command.Parameters.Add(new SqlParameter("@IsUrgent", SqlDbType.Bit) { Value = purchaseRequest.IsUrgent });
        command.Parameters.Add(SqlNVarChar("@Reason", -1, purchaseRequest.Reason));
        command.Parameters.Add(SqlNVarChar("@RecommendedSupplier", 400, purchaseRequest.RecommendedSupplier));
        command.Parameters.Add(SqlDecimal("@TotalAmount", purchaseRequest.TotalAmount));
        command.Parameters.Add(SqlVarChar("@Status", 30, detail.Status));
        command.Parameters.Add(SqlInt("@CreatedBy", ParseInt(detail.UserId)));
        command.Parameters.Add(SqlInt("@UpdatedBy", ParseInt(detail.UserId)));

        object? value = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    private static async Task SavePurchaseRequestDetailsAsync(
        SqlConnection connection,
        int purchaseRequestId,
        DataTable details,
        CancellationToken cancellationToken)
    {
        await using SqlCommand command = new("CDV_PurchaseRequestDetail_Upsert", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(SqlInt("@PurchaseRequestId", purchaseRequestId));
        command.Parameters.Add(new SqlParameter("@Details", SqlDbType.Structured)
        {
            TypeName = "nhvpa3en_vpa01.CDV_PurchaseRequestDetailType",
            Value = details
        });

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<int> SavePurchaseAsync(
        SqlConnection connection,
        NaverWorksApprovalDetailResponse detail,
        ParsedPurchaseBody purchase,
        CancellationToken cancellationToken)
    {
        string purchaseNo = string.IsNullOrWhiteSpace(detail.DocumentNumber)
            ? $"NW-{detail.ApprovalDocumentId}"
            : detail.DocumentNumber!;

        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = """
            IF EXISTS
            (
                SELECT 1
                FROM [nhvpa3en_vpa01].[CDV_Purchase]
                WHERE ApprovalDocumentId = @ApprovalDocumentId
            )
            BEGIN
                UPDATE [nhvpa3en_vpa01].[CDV_Purchase]
                SET
                    PurchaseNo = @PurchaseNo,
                    Title = @Title,
                    RequesterUserId = @RequesterUserId,
                    RequesterUserName = @RequesterUserName,
                    DepartmentId = @DepartmentId,
                    DepartmentName = @DepartmentName,
                    PurchaseDate = @PurchaseDate,
                    PurchaseOverview = @PurchaseOverview,
                    SelectedSupplierReason = @SelectedSupplierReason,
                    CurrencyCode = @CurrencyCode,
                    TotalAmount = @TotalAmount,
                    VatType = @VatType,
                    PaymentTerms = @PaymentTerms,
                    Status = @Status,
                    DocumentFormId = @DocumentFormId,
                    CreatedTime = @CreatedTime,
                    CompletedTime = @CompletedTime,
                    UpdatedDate = SYSDATETIME()
                WHERE ApprovalDocumentId = @ApprovalDocumentId;
            END
            ELSE
            BEGIN
                INSERT INTO [nhvpa3en_vpa01].[CDV_Purchase]
                (
                    PurchaseNo,
                    Title,
                    RequesterUserId,
                    RequesterUserName,
                    DepartmentId,
                    DepartmentName,
                    PurchaseDate,
                    PurchaseOverview,
                    SelectedSupplierReason,
                    CurrencyCode,
                    TotalAmount,
                    VatType,
                    PaymentTerms,
                    Status,
                    ApprovalDocumentId,
                    DocumentFormId,
                    CreatedTime,
                    CompletedTime,
                    CreatedDate
                )
                VALUES
                (
                    @PurchaseNo,
                    @Title,
                    @RequesterUserId,
                    @RequesterUserName,
                    @DepartmentId,
                    @DepartmentName,
                    @PurchaseDate,
                    @PurchaseOverview,
                    @SelectedSupplierReason,
                    @CurrencyCode,
                    @TotalAmount,
                    @VatType,
                    @PaymentTerms,
                    @Status,
                    @ApprovalDocumentId,
                    @DocumentFormId,
                    @CreatedTime,
                    @CompletedTime,
                    SYSDATETIME()
                );
            END

            SELECT Id
            FROM [nhvpa3en_vpa01].[CDV_Purchase]
            WHERE ApprovalDocumentId = @ApprovalDocumentId;
            """;

        command.Parameters.Add(SqlVarChar("@ApprovalDocumentId", 100, detail.ApprovalDocumentId.ToString(CultureInfo.InvariantCulture)));
        command.Parameters.Add(SqlVarChar("@PurchaseNo", 50, purchaseNo));
        command.Parameters.Add(SqlNVarChar("@Title", 1000, detail.Title));
        command.Parameters.Add(SqlNVarChar("@RequesterUserId", 100, detail.UserId));
        command.Parameters.Add(SqlNVarChar("@RequesterUserName", 400, detail.UserName));
        command.Parameters.Add(SqlNVarChar("@DepartmentId", 100, detail.OrgUnitId));
        command.Parameters.Add(SqlNVarChar("@DepartmentName", 400, detail.OrgUnitName));
        command.Parameters.Add(SqlDate("@PurchaseDate", purchase.PurchaseDate));
        command.Parameters.Add(SqlNVarChar("@PurchaseOverview", -1, purchase.PurchaseOverview));
        command.Parameters.Add(SqlNVarChar("@SelectedSupplierReason", -1, purchase.SelectedSupplierReason));
        command.Parameters.Add(SqlVarChar("@CurrencyCode", 10, purchase.CurrencyCode));
        command.Parameters.Add(SqlDecimal("@TotalAmount", purchase.TotalAmount));
        command.Parameters.Add(SqlNVarChar("@VatType", 100, purchase.VatType));
        command.Parameters.Add(SqlNVarChar("@PaymentTerms", 500, purchase.PaymentTerms));
        command.Parameters.Add(SqlVarChar("@Status", 30, detail.Status));
        command.Parameters.Add(SqlVarChar("@DocumentFormId", 100, detail.DocumentFormId));
        command.Parameters.Add(SqlDateTimeOffset("@CreatedTime", detail.CreatedTime));
        command.Parameters.Add(SqlDateTimeOffset("@CompletedTime", detail.CompletedTime));

        object? value = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    private static async Task SavePurchaseDetailsAsync(
        SqlConnection connection,
        int purchaseId,
        List<ParsedPurchaseDetailBody> details,
        CancellationToken cancellationToken)
    {
        await using SqlCommand deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = "DELETE FROM [nhvpa3en_vpa01].[CDV_PurchaseDetail] WHERE PurchaseId = @PurchaseId;";
        deleteCommand.Parameters.Add(SqlInt("@PurchaseId", purchaseId));
        await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

        foreach (ParsedPurchaseDetailBody detail in details)
        {
            await using SqlCommand insertCommand = connection.CreateCommand();
            insertCommand.CommandText = """
                INSERT INTO [nhvpa3en_vpa01].[CDV_PurchaseDetail]
                (
                    PurchaseId,
                    ItemName,
                    SupplierName,
                    Amount,
                    Delivery,
                    Note
                )
                VALUES
                (
                    @PurchaseId,
                    @ItemName,
                    @SupplierName,
                    @Amount,
                    @Delivery,
                    @Note
                );
                """;

            insertCommand.Parameters.Add(SqlInt("@PurchaseId", purchaseId));
            insertCommand.Parameters.Add(SqlNVarChar("@ItemName", 1000, detail.ItemName));
            insertCommand.Parameters.Add(SqlNVarChar("@SupplierName", 400, detail.SupplierName));
            insertCommand.Parameters.Add(SqlDecimal("@Amount", detail.Amount));
            insertCommand.Parameters.Add(SqlNVarChar("@Delivery", 400, detail.Delivery));
            insertCommand.Parameters.Add(SqlNVarChar("@Note", 1000, detail.Note));

            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task SavePurchaseRequestLinksAsync(
        SqlConnection connection,
        int purchaseId,
        List<ParsedPurchaseRequestLinkBody> links,
        CancellationToken cancellationToken)
    {
        await using SqlCommand deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = "DELETE FROM [nhvpa3en_vpa01].[CDV_PurchaseRequestLink] WHERE PurchaseId = @PurchaseId;";
        deleteCommand.Parameters.Add(SqlInt("@PurchaseId", purchaseId));
        await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

        foreach (ParsedPurchaseRequestLinkBody link in links)
        {
            int? purchaseRequestId = await FindPurchaseRequestIdAsync(
                connection,
                link.ApprovalDocumentId,
                link.DocumentNumber,
                cancellationToken);

            await using SqlCommand insertCommand = connection.CreateCommand();
            insertCommand.CommandText = """
                INSERT INTO [nhvpa3en_vpa01].[CDV_PurchaseRequestLink]
                (
                    PurchaseId,
                    PurchaseRequestId,
                    PurchaseRequestApprovalDocumentId,
                    PurchaseRequestNo,
                    PurchaseRequestTitle
                )
                VALUES
                (
                    @PurchaseId,
                    @PurchaseRequestId,
                    @PurchaseRequestApprovalDocumentId,
                    @PurchaseRequestNo,
                    @PurchaseRequestTitle
                );
                """;

            insertCommand.Parameters.Add(SqlInt("@PurchaseId", purchaseId));
            insertCommand.Parameters.Add(SqlInt("@PurchaseRequestId", purchaseRequestId));
            insertCommand.Parameters.Add(SqlVarChar("@PurchaseRequestApprovalDocumentId", 100, link.ApprovalDocumentId));
            insertCommand.Parameters.Add(SqlVarChar("@PurchaseRequestNo", 50, link.DocumentNumber));
            insertCommand.Parameters.Add(SqlNVarChar("@PurchaseRequestTitle", 1000, link.Title));

            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task<int?> FindPurchaseRequestIdAsync(
        SqlConnection connection,
        string? approvalDocumentId,
        string? documentNumber,
        CancellationToken cancellationToken)
    {
        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT TOP 1 Id
            FROM [nhvpa3en_vpa01].[CDV_PurchaseRequest]
            WHERE (@ApprovalDocumentId IS NOT NULL AND ApprovalDocumentId = @ApprovalDocumentId)
               OR (@DocumentNumber IS NOT NULL AND PRNo = @DocumentNumber);
            """;
        command.Parameters.Add(SqlVarChar("@ApprovalDocumentId", 100, approvalDocumentId));
        command.Parameters.Add(SqlVarChar("@DocumentNumber", 50, documentNumber));

        object? value = await command.ExecuteScalarAsync(cancellationToken);
        return value is null or DBNull ? null : Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    private SqlConnection CreateConnection()
    {
        string? connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        }

        return new SqlConnection(connectionString);
    }

    private static SqlParameter SqlBigInt(string name, long value)
    {
        return new SqlParameter(name, SqlDbType.BigInt) { Value = value };
    }

    private static SqlParameter SqlNVarChar(string name, int size, string? value)
    {
        return new SqlParameter(name, SqlDbType.NVarChar, size) { Value = string.IsNullOrEmpty(value) ? DBNull.Value : value };
    }

    private static SqlParameter SqlVarChar(string name, int size, string? value)
    {
        return new SqlParameter(name, SqlDbType.VarChar, size) { Value = string.IsNullOrEmpty(value) ? DBNull.Value : value };
    }

    private static SqlParameter SqlInt(string name, int? value)
    {
        return new SqlParameter(name, SqlDbType.Int) { Value = value.HasValue ? value.Value : DBNull.Value };
    }

    private static SqlParameter SqlDate(string name, DateTime? value)
    {
        return new SqlParameter(name, SqlDbType.Date) { Value = value.HasValue ? value.Value.Date : DBNull.Value };
    }

    private static SqlParameter SqlTime(string name, TimeSpan? value)
    {
        return new SqlParameter(name, SqlDbType.Time) { Value = value.HasValue ? value.Value : DBNull.Value };
    }

    private static SqlParameter SqlDecimal(string name, decimal? value)
    {
        SqlParameter parameter = new(name, SqlDbType.Decimal)
        {
            Precision = 18,
            Scale = 2,
            Value = value.HasValue ? value.Value : DBNull.Value
        };

        return parameter;
    }

    private static SqlParameter SqlDateTimeOffset(string name, DateTimeOffset? value)
    {
        return new SqlParameter(name, SqlDbType.DateTimeOffset) { Value = value.HasValue ? value.Value : DBNull.Value };
    }

    private static int? ParseInt(string? value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number)
            ? number
            : null;
    }

    private static long ReadLong(JsonElement element, string name)
    {
        if (!TryGetProperty(element, name, out JsonElement value))
        {
            return 0;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out long number))
        {
            return number;
        }

        return value.ValueKind == JsonValueKind.String &&
            long.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number)
            ? number
            : 0;
    }

    private sealed class ParsedLeaveBody
    {
        public string? LeaveType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? LeaveDays { get; set; }
        public string? TimeUsed { get; set; }
        public decimal? DeductHours { get; set; }
        public string? Reason { get; set; }
    }

    private sealed class ParsedPurchaseRequestBody
    {
        public DateTime RequestDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public string CurrencyCode { get; set; } = "VND";
        public string? PurchaseType { get; set; }
        public bool IsUrgent { get; set; }
        public string? Reason { get; set; }
        public string? RecommendedSupplier { get; set; }
        public decimal TotalAmount { get; set; }
        public DataTable Details { get; set; } = new();
    }

    private sealed class ParsedBusinessTripBody
    {
        public string? TripType { get; set; }
        public string? Destination { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Purpose { get; set; }
        public string? Companions { get; set; }
        public string? PassengerCount { get; set; }
        public string? UseTime { get; set; }
        public bool UseCorporateCard { get; set; }
        public decimal TotalAmount { get; set; }
        public List<ParsedBusinessTripDetailBody> Details { get; set; } = [];
    }

    private sealed class ParsedBusinessTripDetailBody
    {
        public decimal? TransportationAmount { get; set; }
        public decimal? LodgingAmount { get; set; }
        public decimal? DailyAllowanceAmount { get; set; }
        public decimal? OtherAmount { get; set; }
        public decimal? TotalAmount { get; set; }
    }

    private sealed class ParsedHiringBody
    {
        public List<ParsedHiringDetailBody> Details { get; set; } = [];
    }

    private sealed class ParsedHiringDetailBody
    {
        public string? PlacementDepartmentId { get; set; }
        public string? PlacementDepartmentName { get; set; }
        public string? JobTitle { get; set; }
        public string? PositionLevel { get; set; }
        public int? HeadCount { get; set; }
        public string? HiringReason { get; set; }
        public string? ReasonDetail { get; set; }
        public DateTime? DesiredStartDate { get; set; }
        public string? SalaryLevel { get; set; }
        public string? Qualification { get; set; }
    }

    private sealed class OrgUnitValue
    {
        public string? OrgUnitId { get; set; }
        public string? OrgUnitName { get; set; }
    }

    private sealed class ParsedExitPermissionBody
    {
        public string? ExitType { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public string? CarrierUserId { get; set; }
        public string? CarrierUserName { get; set; }
        public string? CarrierDepartmentId { get; set; }
        public string? CarrierDepartmentName { get; set; }
        public string? ReceiverCompany { get; set; }
        public string? VehicleNo { get; set; }
        public string? DriverName { get; set; }
        public string? Reason { get; set; }
        public List<ParsedExitPermissionDetailBody> Details { get; set; } = [];
    }

    private sealed class ParsedExitPermissionDetailBody
    {
        public int? LineNo { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public string? Unit { get; set; }
        public decimal? Qty { get; set; }
        public string? Remark { get; set; }
    }

    private sealed class UserValue
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
    }

    private sealed class ParsedCompanyVisitorBody
    {
        public string? CompanyName { get; set; }
        public string? RepresentativeName { get; set; }
        public DateTime? VisitDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? Purpose { get; set; }
        public string? ContactUserId { get; set; }
        public string? ContactUserName { get; set; }
        public List<ParsedCompanyVisitorDetailBody> Details { get; set; } = [];
    }

    private sealed class ParsedCompanyVisitorDetailBody
    {
        public string? VisitorName { get; set; }
        public string? VisitorTitle { get; set; }
        public string? IdentityNo { get; set; }
    }

    private sealed class ParsedPurchaseBody
    {
        public DateTime PurchaseDate { get; set; }
        public string? PurchaseOverview { get; set; }
        public string? SelectedSupplierReason { get; set; }
        public string CurrencyCode { get; set; } = "VND";
        public decimal TotalAmount { get; set; }
        public string? VatType { get; set; }
        public string? PaymentTerms { get; set; }
        public List<ParsedPurchaseDetailBody> Details { get; set; } = [];
        public List<ParsedPurchaseRequestLinkBody> RequestLinks { get; set; } = [];
    }

    private sealed class ParsedPurchaseDetailBody
    {
        public string ItemName { get; set; } = string.Empty;
        public string? SupplierName { get; set; }
        public decimal? Amount { get; set; }
        public string? Delivery { get; set; }
        public string? Note { get; set; }
    }

    private sealed class ParsedPurchaseRequestLinkBody
    {
        public string? ApprovalDocumentId { get; set; }
        public string? DocumentNumber { get; set; }
        public string? Title { get; set; }
    }

    private sealed record NaverWorksSyncHandler(
        string Type,
        string DocumentFormId,
        string OptionName,
        Func<SqlConnection, NaverWorksApprovalDetailResponse, CancellationToken, Task> SaveAsync);
}
