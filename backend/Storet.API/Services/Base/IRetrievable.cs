namespace Storet.API.Services.Base;

public interface IRetrievable<TModel, TSingleModel, TKey>
{
	public Task<IEnumerable<TModel>> GetAllAsync ();
	public Task<TSingleModel?> GetOneAsync (TKey key);
}