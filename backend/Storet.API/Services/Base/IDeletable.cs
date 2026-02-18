namespace Storet.API.Services.Base;

public interface IDeletable<TKey>
{
	public Task<bool> DeleteAsync (TKey key);
}