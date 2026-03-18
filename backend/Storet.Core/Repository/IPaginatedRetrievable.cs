using Storet.Core.Utils;

namespace Storet.Core.Repository;

public interface IPaginatedRetrievable<TModel, TKey, TPaginationKey>
{
	public Task<IEnumerable<TModel>> GetAllAsync (Query<TPaginationKey> query);
	public Task<TPaginationKey?> GetPreviousKeyAsync (Query<TPaginationKey> query);
	public Task<TModel?> GetOneAsync (TKey key);
}