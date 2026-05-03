namespace Storet.Core.Service;

public interface IBulkExistenceCheckable<TKey>
{
	public Task<bool> AllExistAsync (IEnumerable<TKey> keys);
}