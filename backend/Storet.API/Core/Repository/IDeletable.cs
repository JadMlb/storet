namespace Storet.API.Core.Repository;

public interface IDeletable<TKey>
{
	public Task<bool> DeleteAsync (TKey key);
}