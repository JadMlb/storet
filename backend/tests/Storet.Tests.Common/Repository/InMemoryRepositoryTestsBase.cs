using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Storet.Tests.Common.Repository;

public abstract class InMemoryRepositoryTestsBase<TContext, TRepository> : RepositoryTestsBase<TContext, TRepository> where TContext : DbContext
{
	public InMemoryRepositoryTestsBase (ITestOutputHelper output) : base (output) {}
	
	protected override TContext InitDbContext ()
	{
		var options = new DbContextOptionsBuilder<TContext>()
							.UseInMemoryDatabase (databaseName: Guid.NewGuid().ToString())
							.Options;
		return InitDbContextWithOptions (options);
	}
	
	protected abstract TContext InitDbContextWithOptions (DbContextOptions<TContext> options);
}