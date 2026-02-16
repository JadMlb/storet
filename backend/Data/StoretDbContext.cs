using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Storet.Backend.Models;

namespace Storet.Backend.Data;

public class StoretDbContext : DbContext
{
	public StoretDbContext (DbContextOptions<StoretDbContext> options) : base (options) {}

	public DbSet<Category> Categories;

	protected override void OnModelCreating (ModelBuilder modelBuilder)
	{
		base.OnModelCreating (modelBuilder);

		modelBuilder.ApplyConfigurationsFromAssembly (Assembly.GetExecutingAssembly());
	}
}