using Microsoft.EntityFrameworkCore;
using Storet.API.Data;

namespace Storet.Tests.Repositories.Config;

public abstract class InMemoryRepositoryTestsBase<T> : RepositoryTestsBase<T>
{
	protected override DbContextOptions<StoretDbContext> InitDatabaseOptions()
	{
		return new DbContextOptionsBuilder<StoretDbContext>()
					.UseInMemoryDatabase (databaseName: Guid.NewGuid().ToString())
					.Options;
	}
}