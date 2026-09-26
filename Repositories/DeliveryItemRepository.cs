using ChangdaeVinaPurchasingApi.Helpers;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class DeliveryItemRepository
{
    private const string ConnectionStringName = "DeliveryDbConnection";
    private readonly IConfiguration _configuration;

    public DeliveryItemRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<List<Dictionary<string, object?>>> SelectForDeliveryNoteAsync(
        string? material,
        DateTime deliveryDateFrom,
        DateTime deliveryDateTo)
    {
        string? connectionString = _configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is missing.");
        }

        await using SqlConnection connection = new(connectionString);
        await using SqlCommand command = new("sp_DELIVERY_ITEM_SelectForDeliveryNote", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@MATERIAL", string.IsNullOrWhiteSpace(material) ? DBNull.Value : material));
        command.Parameters.Add(new SqlParameter("@CUSTOMER_DO_DELIVERY_DATE_From", deliveryDateFrom.Date));
        command.Parameters.Add(new SqlParameter("@CUSTOMER_DO_DELIVERY_DATE_To", deliveryDateTo.Date));

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        return await DataReaderHelper.ToDictionaryListAsync(reader);
    }
}
