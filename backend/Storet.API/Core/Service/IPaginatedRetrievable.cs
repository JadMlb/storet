using Storet.API.Core.Utils;

namespace Storet.API.Core.Service;

public interface IPaginatedRetrievable<TModel, TSingleModel, TKey, TPaginationKey>
{
	public Task<PaginatedResponse<TModel, TPaginationKey>> GetAllAsync (Query<TPaginationKey> query);
	public Task<TSingleModel?> GetOneAsync (TKey key);
}