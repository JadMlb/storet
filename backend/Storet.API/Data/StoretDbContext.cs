using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Storet.API.Models;

namespace Storet.API.Data;

public class StoretDbContext : DbContext
{
	public StoretDbContext (DbContextOptions<StoretDbContext> options) : base (options) {}

	public DbSet<Category> Categories { get; set; }
	public DbSet<CategoryHierarchy> CategoryHierarchies { get; set; }
	public DbSet<Item> Items { get; set; }
	public DbSet<ItemCategory> ItemsCategories { get; set; }

	protected override void OnModelCreating (ModelBuilder modelBuilder)
	{
		base.OnModelCreating (modelBuilder);

		modelBuilder.ApplyConfigurationsFromAssembly (Assembly.GetExecutingAssembly());
	}
}