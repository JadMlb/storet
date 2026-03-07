namespace Storet.API.Core.Repository;

public interface IExistenceCheckable<TKey>
{
	public Task<bool> ExistsAsync (TKey key);
}