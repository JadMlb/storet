namespace Storet.Core.Repository;

public interface IInsertable<TModel>
{
	public Task<TModel?> InsertAsync (TModel model);
}