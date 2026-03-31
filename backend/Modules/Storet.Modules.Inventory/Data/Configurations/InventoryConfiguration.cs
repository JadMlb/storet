using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Data.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Models.Inventory>
{
	public void Configure (EntityTypeBuilder<Models.Inventory> builder)
	{
		builder.ToTable ("inventory", schema: "inventory");
		
		builder.HasKey (i => new {i.ItemId, i.UserId})
				.HasName ("pk_inventory");
		builder.Property (i => i.ItemId)
				.HasColumnName ("item_id");
		builder.Property (i => i.UserId)
				.HasColumnName ("user_id");
		builder.Property (i => i.QuantityInStock)
				.HasColumnName ("quantity_in_stock");
		builder.Property (i => i.MinQuantity)
				.HasColumnName ("min_quantity");
		builder.Property (i => i.MaxQuantity)
				.HasColumnName ("max_quantity");
		builder.Property (i => i.Status)
				.HasColumnName ("status")
				.HasColumnType ("inventory.statuses")
				.HasDefaultValue (Status.Empty);
	}
}