using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
	public void Configure (EntityTypeBuilder<Item> builder)
	{
		builder.ToTable (
			"items",
			schema: "items_catalogue",
			t => t.HasCheckConstraint ("ck_items_positive_quantity", "quantity > 0")
		);

		builder.HasKey (i => i.Id)
				.HasName ("pk_items");
		builder.Property (i => i.Id)
				.HasColumnName ("id")
				.ValueGeneratedOnAdd();
		builder.Property (i => i.Name)
				.HasColumnName ("name")
				.HasColumnType ("text");
		builder.Property (i => i.Description)
				.HasColumnName ("description")
				.HasColumnType ("text")
				.IsRequired (false);
		builder.Property (i => i.Quantity)
				.HasColumnName ("quantity");
		builder.Property (i => i.Unit)
				.HasColumnName ("unit")
				.HasColumnType ("items_catalogue.units")
				.HasDefaultValue (Unit.Unit);
		builder.Property (i => i.IsComponent)
				.HasColumnName ("is_component")
				.HasDefaultValue (false);
		builder.Property (i => i.UserId)
				.HasColumnName ("user_id");

		builder.HasIndex (i => i.Name)
				.HasDatabaseName ("idx_uniq_items_name")
				.IsUnique();

		builder.HasIndex (i => i.UserId)
				.HasDatabaseName ("idx_items_user_ids");
	}
}