using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/payments")
            .WithTags("Payments");

        group.MapGet("", async (PaymentRepository repository) =>
        {
            return await ApiResponseHelper.HandleAsync(repository.GetAllAsync);
        });

        group.MapGet("{id:int}", async (PaymentRepository repository, int id) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByIdAsync(id));
        });

        group.MapGet("by-enrollment/{enrollmentId:int}", async (PaymentRepository repository, int enrollmentId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByEnrollmentIdAsync(enrollmentId));
        });

        group.MapPost("", async (PaymentRepository repository, Dictionary<string, JsonElement> body) =>
        {
            SqlParameter[] parameters =
            [
                    SqlParameterHelper.Int("Id", body),
                    SqlParameterHelper.Int("UserId", body),
                    SqlParameterHelper.Int("StudentId", body),
                    SqlParameterHelper.Int("EnrollmentId", body),
                    SqlParameterHelper.Decimal("Amount", body),
                    SqlParameterHelper.String("PaymentMethod", body),
                    SqlParameterHelper.String("Note", body),
                    SqlParameterHelper.String("PaymentType", body)
            ];

            return await ApiResponseHelper.HandleAsync(() => repository.UpsertAsync(parameters));
        });


        group.MapDelete("{id:int}", async (PaymentRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }
}
