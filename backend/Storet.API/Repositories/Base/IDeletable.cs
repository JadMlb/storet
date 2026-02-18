namespace Storet.API.Repositories.Base;

public interface IDeletable<TKey>
{
	public Task<bool> DeleteAsync (TKey key);
}