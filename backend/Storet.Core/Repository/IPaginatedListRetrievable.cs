using Storet.Core.Utils;

namespace Storet.Core.Repository;

public interface IPaginatedListRetrievable<TModel, TKey, TPaginationKey>
{
	public Task<IEnumerable<TModel>> GetAllAsync (Query<TPaginationKey> query, Guid userId);
	public Task<TPaginationKey?> GetPreviousKeyAsync (Query<TPaginationKey> query, Guid userId);
}