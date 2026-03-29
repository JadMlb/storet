using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Data.Configurations;

public class CategoryHierarchyConfiguration : IEntityTypeConfiguration<CategoryHierarchy>
{
	public void Configure (EntityTypeBuilder<CategoryHierarchy> builder)
	{
		builder.ToView ("v_categories", schema: "items_catalogue");
		builder.HasNoKey();

		builder.Property (c => c.Id)
				.HasColumnName ("id");
		builder.Property (c => c.Label)
				.HasColumnName ("label");
		builder.Property (c => c.ParentCategoryId)
				.HasColumnName ("parent_id");
		builder.Property (c => c.Level)
				.HasColumnName ("level");
		builder.Property (c => c.UserId)
				.HasColumnName ("user_id");
	}
}