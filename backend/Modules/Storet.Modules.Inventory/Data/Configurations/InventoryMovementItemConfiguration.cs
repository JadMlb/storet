using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Data.Configurations;

public class InventoryMovementItemConfiguration : IEntityTypeConfiguration<InventoryMovementItem>
{
	public void Configure (EntityTypeBuilder<InventoryMovementItem> builder)
	{
		builder.ToTable (
			"inventory_movement_items",
			schema: "inventory",
			t => t.HasCheckConstraint ("ck_inventory_movement_items_positive_quantity", "quantity > 0")
		);
		
		builder.HasKey (i => i.MovementId)
				.HasName ("pk_inventory_movement_items");
		builder.Property (m => m.MovementId)
				.HasColumnName ("movement_id");
		builder.Property (m => m.ItemId)
				.HasColumnName ("item_id");
		builder.Property (m => m.UserId)
				.HasColumnName ("user_id");
		builder.Property (m => m.Quantity)
				.HasColumnName ("quantity");
		builder.HasOne<InventoryMovement>()
				.WithMany (m => m.Items)
				.HasForeignKey (i => i.MovementId)
				.HasConstraintName ("fk_inventory_movement_items_movements");
	}
}