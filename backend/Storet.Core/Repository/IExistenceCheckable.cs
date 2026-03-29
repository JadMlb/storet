namespace Storet.Core.Repository;

public interface IExistenceCheckable<TKey>
{
	public Task<bool> ExistsAsync (TKey key, Guid userId);
}