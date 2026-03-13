using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storet.API.ItemsCatalogue.Models;

namespace Storet.API.ItemsCatalogue.Data.Configurations;

public class ItemCompositionConfiguration : IEntityTypeConfiguration<ItemComposition>
{
	public void Configure (EntityTypeBuilder<ItemComposition> builder)
	{
		builder.ToTable ("items_compositions", schema: "items_catalogue");
		
		builder.HasKey (c => new {c.ParentItemId, c.ComponentItemId})
				.HasName ("pk_items_compositions");
				
		builder.Property (c => c.ParentItemId)
				.HasColumnName ("parent_item_id");
		builder.Property (c => c.ComponentItemId)
				.HasColumnName ("component_item_id");
		
		builder.HasOne<Item>()
				.WithMany (i => i.Components)
				.HasForeignKey (c => c.ParentItemId)
				.HasConstraintName ("fk_items_components_parent_item");
		builder.HasOne (c => c.ComponentItem)
				.WithMany()
				.HasForeignKey (c => c.ComponentItemId)
				.HasConstraintName ("fk_items_components_component_item");
	}
}