namespace Storet.API.Core.Repository;

public interface IUpdatable<TModel, TKey>
{
	public Task<TModel?> UpdateAsync (TKey key, TModel model);
}