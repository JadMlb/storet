namespace Storet.Backend.Repositories.Base;

public interface IRetrievable<TModel, TKey>
{
	public Task<IEnumerable<TModel>> GetAllAsync ();
	public Task<TModel?> GetOneAsync (TKey key);
}