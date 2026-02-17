namespace Storet.API.Repositories.Base;

public interface IUpdatable<TModel, TKey>
{
	public Task<TModel?> UpdateAsync (TKey key, TModel model);
}