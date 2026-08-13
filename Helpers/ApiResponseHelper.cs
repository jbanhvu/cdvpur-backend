using ChangdaeVinaPurchasingApi.Models;

namespace ChangdaeVinaPurchasingApi.Helpers;

public static class ApiResponseHelper
{
    public static IResult Ok<T>(T data, string? message = null)
    {
        return Results.Ok(ApiResponse<T>.Ok(data, message));
    }

    public static IResult Fail(string message, int statusCode = StatusCodes.Status400BadRequest)
    {
        return Results.Json(
            ApiResponse<object?>.Fail(message),
            statusCode: statusCode);
    }

    public static async Task<IResult> HandleAsync<T>(Func<Task<T>> action)
    {
        try
        {
            T data = await action();

            return Ok(data);
        }
        catch (Exception ex)
        {
            return Fail(ex.InnerException?.Message ?? ex.Message);
        }
    }
}
