using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storet.API.Models;

namespace Storet.API.Data.Configurations;

public class CategoryHierarchyConfiguration : IEntityTypeConfiguration<CategoryHierarchy>
{
	public void Configure (EntityTypeBuilder<CategoryHierarchy> builder)
	{
		builder.ToView ("v_categories");
		builder.HasNoKey();

		builder.Property (c => c.Id)
				.HasColumnName ("id");
		builder.Property (c => c.Label)
				.HasColumnName ("label");
		builder.Property (c => c.ParentCategoryId)
				.HasColumnName ("parent_id");
		builder.Property (c => c.Level)
				.HasColumnName ("level");
	}
}