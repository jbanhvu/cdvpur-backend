using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class SupplierEndpoints
{
    public static IEndpointRouteBuilder MapSupplierEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/suppliers")
            .WithTags("Suppliers");

        group.MapGet("", async (SupplierRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (SupplierRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapPost("", async (SupplierRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.NullableInt("Id", body, -1),
                    SqlParameterHelper.NullableInt("UserId", body, 0),
                    SqlParameterHelper.String("Name", body),
                    SqlParameterHelper.NullableString("Code", body),
                    SqlParameterHelper.NullableString("Phone", body),
                    SqlParameterHelper.NullableString("Email", body),
                    SqlParameterHelper.NullableString("Address", body),
                    SqlParameterHelper.NullableString("TaxCode", body),
                    SqlParameterHelper.NullableString("ContactPerson", body),
                    SqlParameterHelper.NullableString("BankAccount", body),
                    SqlParameterHelper.NullableString("BankName", body),
                    SqlParameterHelper.NullableBool("IsActive", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapPut("{id:int}", async (SupplierRepository repository, int id, Dictionary<string, JsonElement> body) =>
        {
            SqlParameterHelper.SetInt("Id", body, id);
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.NullableInt("Id", body, -1),
                    SqlParameterHelper.NullableInt("UserId", body, 0),
                    SqlParameterHelper.String("Name", body),
                    SqlParameterHelper.NullableString("Code", body),
                    SqlParameterHelper.NullableString("Phone", body),
                    SqlParameterHelper.NullableString("Email", body),
                    SqlParameterHelper.NullableString("Address", body),
                    SqlParameterHelper.NullableString("TaxCode", body),
                    SqlParameterHelper.NullableString("ContactPerson", body),
                    SqlParameterHelper.NullableString("BankAccount", body),
                    SqlParameterHelper.NullableString("BankName", body),
                    SqlParameterHelper.NullableBool("IsActive", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });

        group.MapDelete("{id:int}", async (SupplierRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}
