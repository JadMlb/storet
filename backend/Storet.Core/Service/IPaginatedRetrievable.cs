using Storet.Core.Utils;

namespace Storet.Core.Service;

public interface IPaginatedRetrievable<TModel, TSingleModel, TKey, TPaginationKey>
{
	public Task<PaginatedResponse<TModel, TPaginationKey>> GetAllAsync (Query<TPaginationKey> query);
	public Task<TSingleModel?> GetOneAsync (TKey key);
}