namespace Storet.API.Services.Base;

public interface IUpdatable<TRetrieveModel, TKey, TUpdateModel>
{
	public Task<TRetrieveModel?> UpdateAsync (TKey key, TUpdateModel model);
}