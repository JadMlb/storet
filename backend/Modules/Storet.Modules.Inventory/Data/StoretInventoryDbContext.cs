using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Data;

public class StoretInventoryDbContext : DbContext
{
	public StoretInventoryDbContext (DbContextOptions<StoretInventoryDbContext> options) : base (options) {}
	
	public DbSet<StorageLocation> StorageLocations { get; set; }
	public DbSet<InventoryMovement> InventoryMovements { get; set; }
	public DbSet<Models.Inventory> Inventories { get; set; }
	
	protected override void OnModelCreating (ModelBuilder builder)
	{
		base.OnModelCreating (builder);
		
		builder.ApplyConfigurationsFromAssembly (Assembly.GetExecutingAssembly());
		
		if (Database.IsNpgsql())
		{
			builder.HasCollation ("name_case_insensitive", locale: "und-u-ks-level1", provider: "icu", deterministic: false);
			builder.Entity<StorageLocation>()
					.Property (l => l.Name)
					.UseCollation ("name_case_insensitive");
		}
		else
		{
			builder.Entity<Models.Inventory>()
					.Property (i => i.Status)
					.HasColumnType ("text")
					.HasConversion (
						s => s.ToString().ToLowerInvariant(),
						s => Enum.Parse<Status> (s, true)
					);
			
			builder.Entity<InventoryMovement>()
					.Property (m => m.Direction)
					.HasColumnType ("text")
					.HasConversion (
						s => s.ToString().ToLowerInvariant(),
						s => Enum.Parse<MovementDirection> (s, true)
					);
			builder.Entity<InventoryMovement>()
					.Property (m => m.Source)
					.HasColumnType ("text")
					.HasConversion (
						s => s.ToString().ToLowerInvariant(),
						s => Enum.Parse<MovementSource> (s, true)
					);
			builder.Entity<InventoryMovement>()
					.Property (m => m.ExecutedAt)
					.HasColumnType ("text")
					.HasConversion (
						s => s.ToString ("O"),
						s => DateTimeOffset.Parse (s)
					);
		}
	}
}