namespace Storet.Core.Repository;

public interface IRetrievable<TModel, TKey>
{
	public Task<IEnumerable<TModel>> GetAllAsync (Guid userId);
	public Task<TModel?> GetOneAsync (TKey key, Guid userId);
}