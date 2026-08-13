using ChangdaeVinaPurchasingApi.Contracts;
using ChangdaeVinaPurchasingApi.Endpoints;
using ChangdaeVinaPurchasingApi.Models;
using ChangdaeVinaPurchasingApi.Models.NaverWorks;
using ChangdaeVinaPurchasingApi.Repositories;
using ChangdaeVinaPurchasingApi.Services;
using ChangdaeVinaPurchasingApi.Services.NaverWorks;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "AllowVueApp";

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.Configure<GoogleDriveOptions>(
    builder.Configuration.GetSection("GoogleDrive"));
builder.Services.Configure<NaverWorksOptions>(
    builder.Configuration.GetSection("NaverWorks"));

try
{
    if (FirebaseApp.DefaultInstance is null)
    {
        string firebaseCredentialPath = Path.Combine(
            builder.Environment.ContentRootPath,
            "Configs",
            "mrsol-be32d-firebase-adminsdk-fbsvc-70b8651641.json");

        using FileStream firebaseCredentialStream = File.OpenRead(firebaseCredentialPath);
        ServiceAccountCredential firebaseCredential = CredentialFactory
            .FromStream<ServiceAccountCredential>(firebaseCredentialStream);

        FirebaseApp.Create(new AppOptions()
        {
            Credential = firebaseCredential.ToGoogleCredential(),
        });

        Console.WriteLine("Firebase initialized successfully");
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    throw;
}

builder.Services.AddScoped<BishopRepository>();
builder.Services.AddScoped<ProvinceRepository>();
builder.Services.AddScoped<AttendanceRepository>();
builder.Services.AddScoped<ApprovalActionHistoryRepository>();
builder.Services.AddScoped<ApprovalWorkflowStepRepository>();
builder.Services.AddScoped<AuditLogRepository>();
builder.Services.AddScoped<BranchRepository>();
builder.Services.AddScoped<ClassRepository>();
builder.Services.AddScoped<ClassSessionRepository>();
builder.Services.AddScoped<CouponRepository>();
builder.Services.AddScoped<CourseRepository>();
builder.Services.AddScoped<CourseSessionRepository>();
builder.Services.AddScoped<CustomerRepository>();
builder.Services.AddScoped<DepartmantRepository>();
builder.Services.AddScoped<DictionaryRepository>();
builder.Services.AddScoped<EmployeeLeaveRepository>();
builder.Services.AddScoped<EnrollmentRepository>();
builder.Services.AddScoped<EnrollmentScheduleRepository>();
builder.Services.AddScoped<ExpenseRepository>();
builder.Services.AddScoped<FeedbackRepository>();
builder.Services.AddScoped<FunctionRepository>();
builder.Services.AddScoped<HealingAssessmentRepository>();
builder.Services.AddScoped<InvoiceRepository>();
builder.Services.AddScoped<InvoiceDetailRepository>();
builder.Services.AddScoped<InventoryReportRepository>();
builder.Services.AddScoped<LeadRepository>();
builder.Services.AddScoped<LevelRepository>();
builder.Services.AddScoped<MaterialRepository>();
builder.Services.AddScoped<MaterialTypeRepository>();
builder.Services.AddScoped<NewsRepository>();
builder.Services.AddScoped<NewsCategoryRepository>();
builder.Services.AddScoped<NotificationRepository>();
builder.Services.AddScoped<PaymentRepository>();
builder.Services.AddScoped<PermissionRepository>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<ProductCategoryRepository>();
builder.Services.AddScoped<PurchaseOrderRepository>();
builder.Services.AddScoped<PurchaseOrderDetailRepository>();
builder.Services.AddScoped<RecycleInRepository>();
builder.Services.AddScoped<RecycleInDetailRepository>();
builder.Services.AddScoped<RecycleOutRepository>();
builder.Services.AddScoped<RecycleOutDetailRepository>();
builder.Services.AddScoped<RevenueRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<RoleFunctionPermissionRepository>();
builder.Services.AddScoped<RoomRepository>();
builder.Services.AddScoped<SongRepository>();
builder.Services.AddScoped<SongApprovalRequestRepository>();
builder.Services.AddScoped<StudentRepository>();
builder.Services.AddScoped<StudentSessionRepository>();
builder.Services.AddScoped<StockInRepository>();
builder.Services.AddScoped<StockInDetailRepository>();
builder.Services.AddScoped<StockOutRepository>();
builder.Services.AddScoped<StockOutDetailRepository>();
builder.Services.AddScoped<SupplierRepository>();
builder.Services.AddScoped<TeacherRepository>();
builder.Services.AddScoped<TeacherSalaryRepository>();
builder.Services.AddScoped<UnitRepository>();
builder.Services.AddScoped<UserFCMTokenRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<FirebaseNotificationService>();
builder.Services.AddScoped<IGoogleDriveService, GoogleDriveService>();
builder.Services.AddScoped<HikvisionAttendanceService>();
builder.Services.AddScoped<NaverWorksAuthService>();
builder.Services.AddScoped<NaverWorksApprovalService>();
builder.Services.AddScoped<NaverWorksSyncService>();
builder.Services.AddScoped<RoleFunctionPermissionService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri))
                {
                    return false;
                }

                return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                    uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                    uri.Host.Equals("vpatek.com", StringComparison.OrdinalIgnoreCase) ||
                    uri.Host.EndsWith(".vpatek.com", StringComparison.OrdinalIgnoreCase);
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    string? origin = context.Request.Headers.Origin;
    string? accessControlRequestMethod = context.Request.Headers.AccessControlRequestMethod;

    if (!string.IsNullOrWhiteSpace(origin) || !string.IsNullOrWhiteSpace(accessControlRequestMethod))
    {
        app.Logger.LogInformation(
            "CORS request: Method={Method}, Path={Path}, Origin={Origin}, AccessControlRequestMethod={AccessControlRequestMethod}",
            context.Request.Method,
            context.Request.Path,
            origin,
            accessControlRequestMethod);
    }

    await next();
});

app.UseCors(CorsPolicyName);

app.Use(async (context, next) =>
{
    PathString path = context.Request.Path;

    if (path.HasValue &&
        path.Value!.Length > 1 &&
        path.Value.EndsWith("/", StringComparison.Ordinal))
    {
        context.Request.Path = new PathString(path.Value.TrimEnd('/'));
    }

    await next();
});

app.MapMethods("/{*path}", ["OPTIONS"], () => Results.NoContent())
    .RequireCors(CorsPolicyName);

app.MapBishopEndpoints();
app.MapProvinceEndpoints();
app.MapAttendanceEndpoints();
app.MapApprovalActionHistoryEndpoints();
app.MapApprovalWorkflowStepEndpoints();
app.MapAuditLogEndpoints();
app.MapBranchEndpoints();
app.MapClassEndpoints();
app.MapClassSessionEndpoints();
app.MapCouponEndpoints();
app.MapCourseEndpoints();
app.MapCourseSessionEndpoints();
app.MapCustomerEndpoints();
app.MapDepartmantEndpoints();
app.MapDictionaryEndpoints();
app.MapEnrollmentEndpoints();
app.MapEnrollmentScheduleEndpoints();
app.MapExpenseEndpoints();
app.MapFeedbackEndpoints();
app.MapFunctionEndpoints();
app.MapHealingAssessmentEndpoints();
app.MapInvoiceEndpoints();
app.MapInvoiceDetailEndpoints();
app.MapReportEndpoints();
app.MapLeadEndpoints();
app.MapLevelEndpoints();
app.MapMaterialEndpoints();
app.MapMaterialTypeEndpoints();
app.MapNewsEndpoints();
app.MapNewsCategoryEndpoints();
app.MapNotificationTypeEndpoints();
app.MapPaymentEndpoints();
app.MapPermissionEndpoints();
app.MapProductEndpoints();
app.MapProductCategoryEndpoints();
app.MapPurchaseOrderEndpoints();
app.MapPurchaseOrderDetailEndpoints();
app.MapRecycleInEndpoints();
app.MapRecycleInDetailEndpoints();
app.MapRecycleOutEndpoints();
app.MapRecycleOutDetailEndpoints();
app.MapRevenueEndpoints();
app.MapRoleEndpoints();
app.MapRoleFunctionPermissionEndpoints();
app.MapRoomEndpoints();
app.MapSongEndpoints();
app.MapSongApprovalRequestEndpoints();
app.MapStudentEndpoints();
app.MapStudentNotificationEndpoints();
app.MapStudentSessionEndpoints();
app.MapStockInEndpoints();
app.MapStockInDetailEndpoints();
app.MapStockOutEndpoints();
app.MapStockOutDetailEndpoints();
app.MapSupplierEndpoints();
app.MapTeacherEndpoints();
app.MapTeacherSalaryEndpoints();
app.MapUnitEndpoints();
app.MapUserFCMTokenEndpoints();
app.MapUserNotificationEndpoints();
app.MapUserEndpoints();
app.MapNotificationEndpoints();
app.MapControllers();

app.Run();
