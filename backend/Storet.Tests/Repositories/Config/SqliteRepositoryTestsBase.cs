using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Storet.API.Data;

namespace Storet.Tests.Repositories.Config;

public abstract class SqliteRepositoryTestsBase<T> : RepositoryTestsBase<T>
{
	protected SqliteConnection connection = new ("Filename=:memory:");
	
	protected override DbContextOptions<StoretDbContext> InitDatabaseOptions()
	{
		connection.Open();
		
		return new DbContextOptionsBuilder<StoretDbContext>()
					.UseSqlite (connection)
					.Options;
	}
	
	public override void Dispose ()
	{
		base.Dispose();
		connection.Dispose();
	}
}