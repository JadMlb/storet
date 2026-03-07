namespace Storet.API.Core.Service;

public interface IInsertable<TRetrieveModel, TInsertModel>
{
	public Task<TRetrieveModel?> InsertAsync (TInsertModel model);
}