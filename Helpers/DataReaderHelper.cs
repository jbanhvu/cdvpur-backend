using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Helpers;

public static class DataReaderHelper
{
    public static async Task<List<Dictionary<string, object?>>> ToDictionaryListAsync(SqlDataReader reader)
    {
        List<Dictionary<string, object?>> rows = new();

        while (await reader.ReadAsync())
        {
            Dictionary<string, object?> row = new();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = await reader.IsDBNullAsync(i)
                    ? null
                    : reader.GetValue(i);
            }

            rows.Add(row);
        }

        return rows;
    }
}
