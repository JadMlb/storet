namespace Storet.API.Repositories.Base;

public interface IExistenceCheckable<TKey>
{
	public Task<bool> ExistsAsync (TKey key);
}