using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Storet.Tests.Common.Repository;

public abstract class SqliteRepositoryTestsBase<TContext, TRepository> : RepositoryTestsBase<TContext, TRepository> where TContext : DbContext
{
	protected SqliteConnection connection = new ("Filename=:memory:");
	
	protected override TContext InitDbContext ()
	{
		connection.Open();
		
		var options = new DbContextOptionsBuilder<TContext>()
							.UseSqlite (connection)
							.Options;
							
		return InitDbContextWithOptions (options);
	}
	
	protected abstract TContext InitDbContextWithOptions (DbContextOptions<TContext> options);
	
	public override void Dispose ()
	{
		base.Dispose();
		connection.Dispose();
	}
}