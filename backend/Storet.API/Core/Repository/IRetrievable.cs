namespace Storet.API.Core.Repository;

public interface IRetrievable<TModel, TKey>
{
	public Task<IEnumerable<TModel>> GetAllAsync ();
	public Task<TModel?> GetOneAsync (TKey key);
}