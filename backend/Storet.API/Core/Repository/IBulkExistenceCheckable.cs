namespace Storet.API.Core.Repository;

public interface IBulkExistenceCheckable<TKey>
{
	public Task<bool> AllExistAsync (IEnumerable<TKey> keys);
}