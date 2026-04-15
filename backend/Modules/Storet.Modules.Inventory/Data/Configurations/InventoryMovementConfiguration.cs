using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Data.Configurations;

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
	public void Configure (EntityTypeBuilder<InventoryMovement> builder)
	{
		builder.ToTable ("inventory_movements", schema: "inventory");
		
		builder.HasKey (m => m.Id)
				.HasName ("pk_inventory_movements");
		builder.Property (m => m.Id)
				.HasColumnName ("id")
				.ValueGeneratedOnAdd();
		builder.Property (m => m.UserId)
				.HasColumnName ("user_id");
		builder.Property (m => m.StorageLocationId)
				.HasColumnName ("storage_location_id");
		builder.HasOne (m => m.StorageLocation)
				.WithMany()
				.HasForeignKey (m => m.StorageLocationId)
				.HasConstraintName ("fk_inventory_movements_storage_locations");
		builder.Property (m => m.NumberOfItems)
				.HasColumnName ("nb_items");
		builder.Property (m => m.ExecutedAt)
				.HasColumnName ("executed_at")
				.HasColumnType ("timestamptz");
		builder.Property (m => m.Direction)
				.HasColumnName ("direction")
				.HasColumnType ("inventory.directions");
		builder.Property (m => m.Source)
				.HasColumnName ("source")
				.HasColumnType ("inventory.sources");
		
		builder.HasIndex (m => m.ExecutedAt)
				.HasDatabaseName ("idx_inventory_movements_executed_at");
		builder.HasIndex (m => m.UserId)
				.HasDatabaseName ("idx_inventory_movements_user");
	}
}