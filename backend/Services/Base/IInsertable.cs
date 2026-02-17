namespace Storet.Backend.Services.Base;

public interface IInsertable<TRetrieveModel, TInsertModel>
{
	public Task<TRetrieveModel?> InsertAsync (TInsertModel model);
}