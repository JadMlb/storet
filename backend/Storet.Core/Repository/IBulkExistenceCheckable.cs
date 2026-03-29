namespace Storet.Core.Repository;

public interface IBulkExistenceCheckable<TKey>
{
	public Task<bool> AllExistAsync (Guid userId, IEnumerable<TKey> keys);
}