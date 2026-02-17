namespace Storet.API.Services.Base;

public interface IDeletable<TModel, TKey>
{
	public Task<TModel?> DeleteAsync (TKey key);
}