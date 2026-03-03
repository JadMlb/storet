using Storet.API.Utils;

namespace Storet.API.Repositories.Base;

public interface IPaginatedRetrievable<TModel, TKey, TPaginationKey>
{
	public Task<IEnumerable<TModel>> GetAllAsync (Query<TPaginationKey> query);
	public Task<TPaginationKey?> GetPreviousKeyAsync (Query<TPaginationKey> query);
	public Task<TModel?> GetOneAsync (TKey key);
}