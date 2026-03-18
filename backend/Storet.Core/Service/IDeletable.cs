namespace Storet.Core.Service;

public interface IDeletable<TKey>
{
	public Task<bool> DeleteAsync (TKey key);
}