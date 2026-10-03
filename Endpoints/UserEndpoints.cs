using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/users")
            .WithTags("Users");

        group.MapGet("", async (UserRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (UserRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-department-code/{departmentCode}", async (UserRepository repository, string departmentCode) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByDepartmentCodeAsync(departmentCode));
        });

        group.MapPost("login", async (UserRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.String("Username", body),
                    SqlParameterHelper.String("PasswordHash", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.LoginAsync(parameters));
        });

        group.MapPost("change-password", async (UserRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.String("OldPasswordHash", body),
                    SqlParameterHelper.String("NewPasswordHash", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.ChangePasswordAsync(parameters));
        });

        group.MapPost("", async (UserRepository repository, IWebHostEnvironment environment, HttpRequest request) =>
        {
            return await ApiResponseHelper.HandleAsync(() => UpsertUserAsync(repository, environment, request));
        });

        group.MapPut("{id:int}", async (UserRepository repository, IWebHostEnvironment environment, HttpRequest request, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => UpsertUserAsync(repository, environment, request, id));
        });

        group.MapDelete("{id:int}", async (UserRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }

    private static async Task<List<Dictionary<string, object?>>> UpsertUserAsync(
        UserRepository repository,
        IWebHostEnvironment environment,
        HttpRequest request,
        int? routeId = null)
    {
        (Dictionary<string, JsonElement> body, IFormFile? avatar) = await ReadUserRequestAsync(request);

        if (routeId.HasValue)
        {
            SqlParameterHelper.SetInt("Id", body, routeId.Value);
        }

        SqlParameter[] parameters = BuildUpsertParameters(body);
        List<Dictionary<string, object?>> result = await repository.UpsertAsync(parameters);

        if (avatar is null || avatar.Length == 0)
        {
            return result;
        }

        int id = GetResultId(result);
        string fileName = await SaveAvatarAsync(environment, id, avatar);
        int? updatedBy = GetOptionalInt(body, "UpdatedBy") ?? GetOptionalInt(body, "UserId");

        return await repository.UpdateAvatarAsync(id, fileName, updatedBy);
    }

    private static SqlParameter[] BuildUpsertParameters(IReadOnlyDictionary<string, JsonElement> body)
    {
        return
        [
            SqlParameterHelper.NullableInt("Id", body, -1),
            SqlParameterHelper.Int("UserId", body),
            SqlParameterHelper.String("FullName", body),
            SqlParameterHelper.String("Username", body),
            SqlParameterHelper.NullableString("PasswordHash", body),
            SqlParameterHelper.String("Phone", body),
            SqlParameterHelper.String("Email", body),
            SqlParameterHelper.Int("RoleID", body),
            SqlParameterHelper.String("RoleName", body),
            SqlParameterHelper.NullableInt("BranchId", body),
            SqlParameterHelper.NullableInt("DepartmentId", body),
            SqlParameterHelper.NullableString("Nationality", body),
            SqlParameterHelper.NullableString("IdentityNumber", body),
            SqlParameterHelper.NullableDate("IdentityIssueDate", body),
            SqlParameterHelper.NullableString("Address", body),
            SqlParameterHelper.NullableString("EmergencyContact", body),
            SqlParameterHelper.NullableString("EmergencyPhone", body),
            SqlParameterHelper.NullableDate("ContractStartDate", body),
            SqlParameterHelper.NullableDate("ContractEndDate", body),
            SqlParameterHelper.NullableString("AttendanceCode", body),
            SqlParameterHelper.NullableString("NaverWorksUserId", body),
            SqlParameterHelper.NullableInt("UpdatedBy", body),
            SqlParameterHelper.NullableString("EmployeeCode", body),
            SqlParameterHelper.NullableInt("Gender", body),
            SqlParameterHelper.NullableDate("DateOfBirth", body),
            SqlParameterHelper.NullableDate("JoinDate", body),
            SqlParameterHelper.NullableDate("ResignDate", body),
            SqlParameterHelper.NullableInt("EmploymentStatus", body),
            SqlParameterHelper.NullableString("Position", body),
            SqlParameterHelper.NullableInt("ManagerId", body),
            SqlParameterHelper.NullableInt("EmploymentType", body),
            SqlParameterHelper.Bool("IsActive", body),
            SqlParameterHelper.NullableString("AvatarUrl", body)
        ];
    }

    private static async Task<(Dictionary<string, JsonElement> Body, IFormFile? Avatar)> ReadUserRequestAsync(HttpRequest request)
    {
        if (request.HasFormContentType)
        {
            IFormCollection form = await request.ReadFormAsync();
            Dictionary<string, JsonElement> body = new(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> item in form)
            {
                body[item.Key] = JsonSerializer.SerializeToElement(item.Value.ToString());
            }

            return (body, form.Files["avatar"] ?? form.Files["Avatar"] ?? form.Files["file"] ?? form.Files["image"]);
        }

        Dictionary<string, JsonElement>? jsonBody = await JsonSerializer.DeserializeAsync<Dictionary<string, JsonElement>>(
            request.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return (jsonBody ?? new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase), null);
    }

    private static async Task<string> SaveAvatarAsync(IWebHostEnvironment environment, int id, IFormFile avatar)
    {
        string folder = Path.Combine(environment.ContentRootPath, "img", "avatar");
        Directory.CreateDirectory(folder);

        string fileName = $"{id}.jpg";
        string filePath = Path.Combine(folder, fileName);

        await using FileStream stream = File.Create(filePath);
        await avatar.CopyToAsync(stream);

        return fileName;
    }

    private static int GetResultId(List<Dictionary<string, object?>> result)
    {
        if (result.Count == 0)
        {
            throw new InvalidOperationException("User upsert did not return an id.");
        }

        foreach (KeyValuePair<string, object?> item in result[0])
        {
            if (!string.Equals(item.Key, "ID", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(item.Key, "Id", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (item.Value is int intValue)
            {
                return intValue;
            }

            if (item.Value is not null && int.TryParse(item.Value.ToString(), out int parsedValue))
            {
                return parsedValue;
            }
        }

        throw new InvalidOperationException("User upsert result does not contain ID.");
    }

    private static int? GetOptionalInt(IReadOnlyDictionary<string, JsonElement> body, string name)
    {
        foreach (KeyValuePair<string, JsonElement> item in body)
        {
            if (!string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (item.Value.ValueKind == JsonValueKind.Number && item.Value.TryGetInt32(out int value))
            {
                return value;
            }

            if (item.Value.ValueKind == JsonValueKind.String && int.TryParse(item.Value.GetString(), out value))
            {
                return value;
            }
        }

        return null;
    }
}
