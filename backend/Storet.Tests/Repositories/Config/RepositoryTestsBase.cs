using Microsoft.EntityFrameworkCore;
using Storet.API.Data;

namespace Storet.Tests.Repositories.Config;

public abstract class RepositoryTestsBase<T> : IDisposable
{
	protected readonly StoretDbContext context;
	protected readonly T repository;
	
	public RepositoryTestsBase ()
	{
		var options = InitDatabaseOptions();
		
		context = new StoretDbContext (options);
		repository = InitRepository();
		
		context.Database.EnsureCreated();
	}
	
	protected abstract DbContextOptions<StoretDbContext> InitDatabaseOptions ();
	protected abstract T InitRepository ();

	public virtual void Dispose ()
	{
		context.Database.EnsureDeleted();
		context.Dispose();
	}
}