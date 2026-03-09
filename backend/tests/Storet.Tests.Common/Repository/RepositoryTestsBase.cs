using Microsoft.EntityFrameworkCore;

namespace Storet.Tests.Common.Repository;

public abstract class RepositoryTestsBase<TContext, TRepository> : IDisposable where TContext : DbContext
{
	protected readonly TContext context;
	protected readonly TRepository repository;
	
	public RepositoryTestsBase ()
	{
		context = InitDbContext();
		repository = InitRepository();
		
		context.Database.EnsureCreated();
	}
	
	protected abstract TContext InitDbContext ();
	protected abstract TRepository InitRepository ();

	public virtual void Dispose ()
	{
		context.Database.EnsureDeleted();
		context.Dispose();
	}
}