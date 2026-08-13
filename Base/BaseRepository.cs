using ChangdaeVinaPurchasingApi.Helpers;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ChangdaeVinaPurchasingApi.Base;

public abstract class BaseRepository
{
    private readonly IConfiguration _configuration;

    protected BaseRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected async Task<List<Dictionary<string, object?>>> ExecuteStoredProcedureAsync(
        string storedProcedureName,
        params SqlParameter[] parameters)
    {
        try
        {
            await using SqlConnection connection = CreateConnection();
            await using SqlCommand command = new(storedProcedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            await connection.OpenAsync();

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            return await DataReaderHelper.ToDictionaryListAsync(reader);
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException(
                $"Database error while executing stored procedure '{storedProcedureName}'.",
                ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Unexpected error while executing stored procedure '{storedProcedureName}'.",
                ex);
        }
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
}
