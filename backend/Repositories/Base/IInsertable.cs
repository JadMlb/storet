namespace Storet.Backend.Repositories.Base;

public interface IInsertable<TModel>
{
	public Task<TModel?> InsertAsync (TModel model);
}