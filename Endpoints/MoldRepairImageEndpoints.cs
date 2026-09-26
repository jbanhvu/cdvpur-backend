using ChangdaeVinaPurchasingApi.Helpers;
using ChangdaeVinaPurchasingApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace ChangdaeVinaPurchasingApi.Endpoints;

public static class MoldRepairImageEndpoints
{
    public static IEndpointRouteBuilder MapMoldRepairImageEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/mold-repair-images")
            .WithTags("Mold Repair Images");

        group.MapGet("", async (MoldRepairImageRepository repository, int? id, int? moldRepairLogId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetAllAsync(id, moldRepairLogId));
        });

        group.MapGet("by-log/{moldRepairLogId:int}", async (MoldRepairImageRepository repository, int moldRepairLogId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.GetByMoldRepairLogIdAsync(moldRepairLogId));
        });

        group.MapPost("", async (MoldRepairImageRepository repository, IWebHostEnvironment environment, HttpRequest request) =>
        {
            return await ApiResponseHelper.HandleAsync(async () =>
            {
                if (!request.HasFormContentType)
                {
                    throw new ArgumentException("Content-Type must be multipart/form-data.");
                }

                IFormCollection form = await request.ReadFormAsync();
                int id = ReadInt(form, "Id", -1);
                int moldRepairLogId = ReadInt(form, "MoldRepairLogId");
                string imageType = ReadString(form, "ImageType");
                IFormFile? image = form.Files["image"] ?? form.Files["file"];

                if (image is null || image.Length == 0)
                {
                    throw new ArgumentException("Image file is required. Use form-data field 'image' or 'file'.");
                }

                string fileName = await SaveImageAsync(environment, image);
                SqlParameter[] parameters =
                [
                    new SqlParameter("@Id", id),
                    new SqlParameter("@MoldRepairLogId", moldRepairLogId),
                    new SqlParameter("@ImageType", imageType),
                    new SqlParameter("@ImageUrl", fileName)
                ];

                return await repository.UpsertAsync(parameters);
            });
        });

        group.MapDelete("{id:int}", async (MoldRepairImageRepository repository, int id, int userId) =>
        {
            return await ApiResponseHelper.HandleAsync(() => repository.DeleteAsync(id, userId));
        });

        return app;
    }

    private static async Task<string> SaveImageAsync(IWebHostEnvironment environment, IFormFile image)
    {
        string folder = Path.GetFullPath(Path.Combine(
            environment.ContentRootPath,
            "..",
            "s-erp",
            "img",
            "CDV_MoldRepairImage"));

        Directory.CreateDirectory(folder);

        string originalName = Path.GetFileName(image.FileName);
        string extension = Path.GetExtension(originalName);
        string nameWithoutExtension = Path.GetFileNameWithoutExtension(originalName);
        string safeBaseName = string.Join("_", nameWithoutExtension.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        if (string.IsNullOrWhiteSpace(safeBaseName))
        {
            safeBaseName = "image";
        }

        string fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}_{safeBaseName}{extension}";
        string filePath = Path.Combine(folder, fileName);

        await using FileStream stream = File.Create(filePath);
        await image.CopyToAsync(stream);

        return fileName;
    }

    private static int ReadInt(IFormCollection form, string name, int? defaultValue = null)
    {
        string? value = form[name].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(value))
        {
            if (defaultValue.HasValue)
            {
                return defaultValue.Value;
            }

            throw new ArgumentException($"Field '{name}' is required.");
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
        {
            return result;
        }

        throw new ArgumentException($"Field '{name}' must be an integer.");
    }

    private static string ReadString(IFormCollection form, string name)
    {
        string? value = form[name].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        throw new ArgumentException($"Field '{name}' is required.");
    }
}
