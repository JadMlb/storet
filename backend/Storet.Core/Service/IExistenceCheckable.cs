namespace Storet.Core.Service;

public interface IExistenceCheckable<TKey>
{
	public Task<bool> ExistsAsync (TKey key);
}