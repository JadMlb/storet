using Microsoft.EntityFrameworkCore;

namespace Storet.Tests.Common.Repository;

public abstract class InMemoryRepositoryTestsBase<TContext, TRepository> : RepositoryTestsBase<TContext, TRepository> where TContext : DbContext
{
	protected override TContext InitDbContext ()
	{
		var options = new DbContextOptionsBuilder<TContext>()
							.UseInMemoryDatabase (databaseName: Guid.NewGuid().ToString())
							.Options;
		return InitDbContextWithOptions (options);
	}
	
	protected abstract TContext InitDbContextWithOptions (DbContextOptions<TContext> options);
}