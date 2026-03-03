using Storet.API.Utils;

namespace Storet.API.Services.Base;

public interface IPaginatedRetrievable<TModel, TSingleModel, TKey, TPaginationKey>
{
	public Task<PaginatedResponse<TModel, TPaginationKey>> GetAllAsync (Query<TPaginationKey> query);
	public Task<TSingleModel?> GetOneAsync (TKey key);
}