namespace Storet.API.Repositories.Base;

public interface IInsertable<TModel>
{
	public Task<TModel?> InsertAsync (TModel model);
}