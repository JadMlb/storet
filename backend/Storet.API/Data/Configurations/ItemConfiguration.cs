using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storet.API.Models;

namespace Storet.API.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
	public void Configure (EntityTypeBuilder<Item> builder)
	{
		builder.ToTable ("items");

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

		builder.HasIndex (i => i.Name)
				.HasDatabaseName ("idx_uniq_items_name")
				.IsUnique();
	}
}