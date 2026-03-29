using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Data;

public class StoretItemsCatalogueDbContext : DbContext
{
	public StoretItemsCatalogueDbContext (DbContextOptions<StoretItemsCatalogueDbContext> options) : base (options) {}

	public DbSet<Category> Categories { get; set; }
	public DbSet<CategoryHierarchy> CategoryHierarchies { get; set; }
	public DbSet<Item> Items { get; set; }
	public DbSet<ItemCategory> ItemsCategories { get; set; }
	public DbSet<ItemComposition> ItemsCompositions { get; set; }

	protected override void OnModelCreating (ModelBuilder modelBuilder)
	{
		base.OnModelCreating (modelBuilder);

		modelBuilder.ApplyConfigurationsFromAssembly (Assembly.GetExecutingAssembly());
		
		if (!Database.IsNpgsql())
		{
			modelBuilder.Entity<Item>()
						.Property (i => i.Unit)
						.HasColumnType ("text")
						.HasConversion (
							u => u.ToString().ToLowerInvariant(),
							s => Enum.Parse<Unit> (s, true)
						);
		}
	}
}