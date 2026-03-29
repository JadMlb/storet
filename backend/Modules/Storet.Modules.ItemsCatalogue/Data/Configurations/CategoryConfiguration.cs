using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
	public void Configure (EntityTypeBuilder<Category> builder)
	{
		builder.ToTable ("categories", schema: "items_catalogue");

		builder.HasKey (c => c.Id)
				.HasName ("pk_categories");
		builder.Property (c => c.Id)
				.HasColumnName ("id")
				.ValueGeneratedOnAdd();
		builder.Property (c => c.Label)
				.HasColumnName ("label")
				.HasColumnType ("text");
		builder.Property (c => c.UserId)
				.HasColumnName ("user_id");
		builder.Property (c => c.ParentCategoryId)
				.HasColumnName ("parent_id");
		builder.HasOne (c => c.ParentCategory)
				.WithMany (c => c.SubCategories)
				.HasForeignKey (c => c.ParentCategoryId)
				.OnDelete (DeleteBehavior.Restrict)
				.HasConstraintName ("fk_categories_parent_categ");

		builder.HasIndex (c => c.Label)
				.HasDatabaseName ("idx_uniq_categories_label")
				.IsUnique();
				
		builder.HasIndex (c => c.UserId)
				.HasDatabaseName ("idx_categories_user_ids");
	}
}