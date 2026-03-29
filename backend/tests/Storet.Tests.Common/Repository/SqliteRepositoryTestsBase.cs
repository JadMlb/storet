using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Storet.Tests.Common.Repository;

public abstract class SqliteRepositoryTestsBase<TContext, TRepository> : RepositoryTestsBase<TContext, TRepository> where TContext : DbContext
{
	protected SqliteConnection connection = new ("Filename=:memory:");

	public SqliteRepositoryTestsBase (ITestOutputHelper output) : base (output) {}
	
	protected override TContext InitDbContext ()
	{
		connection.Open();
		
		var options = new DbContextOptionsBuilder<TContext>()
							.UseSqlite (connection)
							.LogTo (output.WriteLine, LogLevel.Information)
							.EnableSensitiveDataLogging()
							.EnableDetailedErrors()
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