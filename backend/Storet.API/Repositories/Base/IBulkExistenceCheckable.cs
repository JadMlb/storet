namespace Storet.API.Repositories.Base;

public interface IBulkExistenceCheckable<TKey>
{
	public Task<bool> AllExistAsync (IEnumerable<TKey> keys);
}