using ChangdaeVinaPurchasingApi.Models;
using ChangdaeVinaPurchasingApi.Repositories;
using FirebaseAdmin.Messaging;

namespace ChangdaeVinaPurchasingApi.Services;

public class FirebaseNotificationService
{
    private const int FcmMaxTokensPerBatch = 500;
    private readonly UserFCMTokenRepository _tokenRepository;

    public FirebaseNotificationService(UserFCMTokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }

    public async Task<PushNotificationResponse> SendToUserAsync(PushNotificationRequest request)
    {
        try
        {
            string? validationError = ValidatePushRequest(request);

            if (validationError is not null)
            {
                return PushNotificationResponse.Fail(validationError);
            }

            List<string> tokens = await _tokenRepository.GetTokensByUserAsync(request.UserId, request.UserType);
            List<string> validTokens = tokens
                .Where(IsValidStoredToken)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            Console.WriteLine($"Push to UserId: {request.UserId}");
            Console.WriteLine($"UserType: {request.UserType ?? "N/A"}");
            Console.WriteLine($"Total Tokens: {tokens.Count}");
            Console.WriteLine($"Valid Tokens: {validTokens.Count}");

            if (validTokens.Count == 0)
            {
                return PushNotificationResponse.Fail("No valid token found");
            }

            return await SendMulticastAsync(
                validTokens,
                request.Title,
                request.Body,
                request.Type,
                request.RefId,
                request.Screen,
                request.UserId);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return PushNotificationResponse.Fail(ex.InnerException?.Message ?? ex.Message);
        }
    }

    public async Task<PushNotificationResponse> SendToRoleAsync(PushRoleNotificationRequest request)
    {
        try
        {
            string? validationError = ValidatePushRoleRequest(request);

            if (validationError is not null)
            {
                return PushNotificationResponse.Fail(validationError);
            }

            List<string> tokens = await _tokenRepository.GetTokensByRoleAsync(request.RoleId, request.BranchId);
            List<string> validTokens = tokens
                .Where(IsValidStoredToken)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            Console.WriteLine($"Push to RoleId: {request.RoleId}");
            Console.WriteLine($"BranchId: {request.BranchId}");
            Console.WriteLine($"Total Tokens: {tokens.Count}");
            Console.WriteLine($"Valid Tokens: {validTokens.Count}");

            if (validTokens.Count == 0)
            {
                return PushNotificationResponse.Fail("No valid token found");
            }

            PushNotificationResponse response = await SendMulticastAsync(
                validTokens,
                request.Title,
                request.Body,
                request.Type,
                request.RefId,
                request.Screen);

            Console.WriteLine($"Push role result: SuccessCount={response.SuccessCount ?? 0}, FailureCount={response.FailureCount ?? 0}");

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return PushNotificationResponse.Fail(ex.InnerException?.Message ?? ex.Message);
        }
    }

    public async Task<PushNotificationResponse> SendToTokenAsync(
        string token,
        string title,
        string body,
        string type = "TEST",
        int refId = 0,
        string screen = "Test")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return PushNotificationResponse.Fail("Title is required");
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return PushNotificationResponse.Fail("Body is required");
            }

            if (!IsValidStoredToken(token))
            {
                return PushNotificationResponse.Fail("No valid token found");
            }

            Message message = new()
            {
                Token = token,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = BuildData(type, refId, screen)
            };

            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

            Console.WriteLine($"Push success: {token}");
            Console.WriteLine($"Firebase response: {response}");

            return PushNotificationResponse.Ok("Push notification sent", 1, 0);
        }
        catch (FirebaseMessagingException ex)
        {
            Console.WriteLine($"Push failed: {token}");
            Console.WriteLine(ex.ToString());

            return PushNotificationResponse.Fail(ex.Message, 0, 1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Push failed: {token}");
            Console.WriteLine(ex.ToString());

            return PushNotificationResponse.Fail(ex.InnerException?.Message ?? ex.Message, 0, 1);
        }
    }

    public async Task<PushNotificationResponse> SendMulticastAsync(
        IReadOnlyList<string> tokens,
        string title,
        string body,
        string type,
        int refId,
        string screen,
        int? userId = null)
    {
        int successCount = 0;
        int failureCount = 0;

        try
        {
            foreach (List<string> batchTokens in Chunk(tokens, FcmMaxTokensPerBatch))
            {
                MulticastMessage message = new()
                {
                    Tokens = batchTokens,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = BuildData(type, refId, screen)
                };

                BatchResponse response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
                successCount += response.SuccessCount;
                failureCount += response.FailureCount;

                Console.WriteLine($"Firebase response: SuccessCount={response.SuccessCount}, FailureCount={response.FailureCount}");
                Console.WriteLine($"Success Count: {response.SuccessCount}");
                Console.WriteLine($"Failure Count: {response.FailureCount}");

                for (int i = 0; i < response.Responses.Count; i++)
                {
                    SendResponse sendResponse = response.Responses[i];
                    string token = batchTokens[i];

                    if (sendResponse.IsSuccess)
                    {
                        Console.WriteLine($"Push success: {token}");
                        Console.WriteLine($"Firebase response: {sendResponse.MessageId}");
                        continue;
                    }

                    Exception? exception = sendResponse.Exception;
                    Console.WriteLine($"Push failed: {token}");
                    Console.WriteLine(exception?.ToString() ?? "Firebase returned failure without exception details.");

                    if (IsInvalidTokenException(exception))
                    {
                        await _tokenRepository.DeleteInvalidTokenAsync(token);
                        Console.WriteLine($"Deleted invalid FCM token: {token}");
                    }
                }
            }

            Console.WriteLine($"Push to UserId: {userId?.ToString() ?? "N/A"}");
            Console.WriteLine($"Total Tokens: {tokens.Count}");

            return PushNotificationResponse.Ok("Push notification sent", successCount, failureCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return PushNotificationResponse.Fail(ex.InnerException?.Message ?? ex.Message, successCount, failureCount);
        }
    }

    private static Dictionary<string, string> BuildData(string type, int refId, string screen)
    {
        return new Dictionary<string, string>
        {
            { "type", type },
            { "refId", refId.ToString() },
            { "screen", screen }
        };
    }

    private static string? ValidatePushRequest(PushNotificationRequest request)
    {
        if (request.UserId <= 0)
        {
            return "UserId is required";
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return "Title is required";
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return "Body is required";
        }

        if (string.IsNullOrWhiteSpace(request.Type))
        {
            return "Type is required";
        }

        if (string.IsNullOrWhiteSpace(request.Screen))
        {
            return "Screen is required";
        }

        return null;
    }

    private static string? ValidatePushRoleRequest(PushRoleNotificationRequest request)
    {
        if (request.RoleId < 0)
        {
            return "RoleId must be greater than or equal to 0";
        }

        if (request.BranchId < 0)
        {
            return "BranchId must be greater than or equal to 0";
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return "Title is required";
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return "Body is required";
        }

        if (string.IsNullOrWhiteSpace(request.Type))
        {
            return "Type is required";
        }

        if (string.IsNullOrWhiteSpace(request.Screen))
        {
            return "Screen is required";
        }

        return null;
    }

    private static bool IsValidStoredToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        string trimmedToken = token.Trim();

        return !trimmedToken.Contains("ERROR:", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(trimmedToken, "Cannot get token", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsInvalidTokenException(Exception? exception)
    {
        if (exception is not FirebaseMessagingException firebaseException)
        {
            return false;
        }

        string message = firebaseException.Message;

        return firebaseException.MessagingErrorCode == MessagingErrorCode.Unregistered
            || firebaseException.MessagingErrorCode == MessagingErrorCode.InvalidArgument
            || message.Contains("registration-token-not-registered", StringComparison.OrdinalIgnoreCase)
            || message.Contains("invalid-registration-token", StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<List<string>> Chunk(IReadOnlyList<string> tokens, int size)
    {
        for (int i = 0; i < tokens.Count; i += size)
        {
            List<string> batch = new();

            for (int j = i; j < Math.Min(i + size, tokens.Count); j++)
            {
                batch.Add(tokens[j]);
            }

            yield return batch;
        }
    }
}
