using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storet.API.ItemsCatalogue.Models;

namespace Storet.API.ItemsCatalogue.Data.Configurations;

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
				.HasDefaultValue (Unit.Unit)
				.HasConversion (
					u => u.ToString().ToLowerInvariant(),
					v => Enum.Parse<Unit> (v, ignoreCase: true)
				);

		builder.HasIndex (i => i.Name)
				.HasDatabaseName ("idx_uniq_items_name")
				.IsUnique();
	}
}