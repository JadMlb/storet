using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Data.Configurations;

public class ItemCategoryConfiguration : IEntityTypeConfiguration<ItemCategory>
{
	public void Configure (EntityTypeBuilder<ItemCategory> builder)
	{
		builder.ToTable ("items_categories", schema: "items_catalogue");
		
		builder.HasKey (i => new {i.ItemId, i.CategoryId})
				.HasName ("pk_items_categories");
		
		builder.Property (i => i.CategoryId)
				.HasColumnName ("category_id");
		builder.Property (i => i.ItemId)
				.HasColumnName ("item_id");
		builder.Property (i => i.UserId)
				.HasColumnName ("user_id");
		builder.HasOne (i => i.Item)
				.WithMany (i => i.ItemCategories)
				.HasForeignKey (i => i.ItemId)
				.HasConstraintName ("fk_items_categories_items");
		builder.HasOne (i => i.Category)
				.WithMany()
				.HasForeignKey (i => i.CategoryId)
				.HasConstraintName ("fk_items_categories_categories");

		builder.HasIndex (c => c.UserId)
				.HasDatabaseName ("idx_items_categories_user_ids");
	}
}