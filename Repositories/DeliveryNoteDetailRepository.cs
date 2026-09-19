using ChangdaeVinaPurchasingApi.Base;
using Microsoft.Data.SqlClient;

namespace ChangdaeVinaPurchasingApi.Repositories;

public class DeliveryNoteDetailRepository : BaseRepository
{
    public DeliveryNoteDetailRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<Dictionary<string, object?>>> GetAllAsync()
    {
        return await ExecuteStoredProcedureAsync(
            "nhvpa3en_vpa01.CDV_DeliveryNoteDetail_Select",
            new SqlParameter("@Id", 0),
            new SqlParameter("@DeliveryNoteId", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByIdAsync(int id)
    {
        return await ExecuteStoredProcedureAsync(
            "nhvpa3en_vpa01.CDV_DeliveryNoteDetail_Select",
            new SqlParameter("@Id", id),
            new SqlParameter("@DeliveryNoteId", DBNull.Value));
    }

    public async Task<List<Dictionary<string, object?>>> GetByDeliveryNoteIdAsync(int deliveryNoteId)
    {
        return await ExecuteStoredProcedureAsync(
            "nhvpa3en_vpa01.CDV_DeliveryNoteDetail_Select",
            new SqlParameter("@Id", 0),
            new SqlParameter("@DeliveryNoteId", deliveryNoteId));
    }

    public async Task<List<Dictionary<string, object?>>> UpsertAsync(params SqlParameter[] parameters)
    {
        return await ExecuteStoredProcedureAsync(
            "nhvpa3en_vpa01.CDV_DeliveryNoteDetail_Upsert",
            parameters);
    }

    public async Task<List<Dictionary<string, object?>>> BulkSaveAsync(int deliveryNoteId, int userId, string detailsJson)
    {
        return await ExecuteStoredProcedureAsync(
            "nhvpa3en_vpa01.CDV_DeliveryNoteDetail_BulkSave",
            new SqlParameter("@DeliveryNoteId", deliveryNoteId),
            new SqlParameter("@UserId", userId),
            new SqlParameter("@DetailsJson", detailsJson));
    }

    public async Task<List<Dictionary<string, object?>>> DeleteAsync(int id, int userId)
    {
        return await ExecuteStoredProcedureAsync(
            "nhvpa3en_vpa01.CDV_DeliveryNoteDetail_Delete",
            new SqlParameter("@Id", id),
            new SqlParameter("@UserId", userId));
    }
}
