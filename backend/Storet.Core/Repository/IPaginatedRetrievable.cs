using Storet.Core.Utils;

namespace Storet.Core.Repository;

public interface IPaginatedRetrievable<TModel, TKey, TPaginationKey> : IPaginatedListRetrievable<TModel, TKey, TPaginationKey>
{
	public Task<TModel?> GetOneAsync (TKey key, Guid userId);
}