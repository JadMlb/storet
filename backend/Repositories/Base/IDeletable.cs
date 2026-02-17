namespace Storet.Backend.Repositories.Base;

public interface IDeletable<TModel, TKey>
{
	public Task<TModel?> DeleteAsync (TKey key);
}