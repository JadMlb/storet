using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Data.Configurations;

public class StorageLocationConfiguration : IEntityTypeConfiguration<StorageLocation>
{
	public void Configure (EntityTypeBuilder<StorageLocation> builder)
	{
		builder.ToTable ("storage_locations", schema: "inventory");
		
		builder.HasKey (l => l.Id)
				.HasName ("pk_storage_locations");
		builder.Property (l => l.Id)
				.HasColumnName ("id")
				.ValueGeneratedOnAdd();
		builder.Property (l => l.Name)
				.HasColumnName ("name")
				.HasColumnType ("text")
				.IsRequired();
		builder.Property (l => l.Description)
				.HasColumnName ("description")
				.HasColumnType ("text")
				.IsRequired (false);
		builder.Property (l => l.UserId)
				.HasColumnName ("user_id");
				
		builder.HasIndex (l => new {l.Name, l.UserId})
				.HasDatabaseName ("idx_uniq_storage_location_names")
				.IsUnique();
	}
}