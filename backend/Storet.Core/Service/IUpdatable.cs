namespace Storet.Core.Service;

public interface IUpdatable<TRetrieveModel, TKey, TUpdateModel>
{
	public Task<TRetrieveModel?> UpdateAsync (TKey key, TUpdateModel model);
}