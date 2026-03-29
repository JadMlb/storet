namespace Storet.Core.Repository;

public interface IUpdatable<TModel, TKey>
{
	public Task<TModel?> UpdateAsync (TKey key, Guid userId, TModel model);
}