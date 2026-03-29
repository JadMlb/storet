using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Storet.Tests.Common.Repository;

public abstract class RepositoryTestsBase<TContext, TRepository> : IDisposable where TContext : DbContext
{
	protected readonly TContext context;
	protected readonly TRepository repository;
	protected readonly ITestOutputHelper output;
	
	public RepositoryTestsBase (ITestOutputHelper output)
	{
		this.output = output;
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