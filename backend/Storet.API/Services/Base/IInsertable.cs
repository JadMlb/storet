namespace Storet.API.Services.Base;

public interface IInsertable<TRetrieveModel, TInsertModel>
{
	public Task<TRetrieveModel?> InsertAsync (TInsertModel model);
}