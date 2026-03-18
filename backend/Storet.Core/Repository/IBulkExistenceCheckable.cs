namespace Storet.Core.Repository;

public interface IBulkExistenceCheckable<TKey>
{
	public Task<bool> AllExistAsync (IEnumerable<TKey> keys);
}