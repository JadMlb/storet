using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Storet.Modules.Inventory.Data;
using Storet.Modules.Inventory.Models;
using Storet.Modules.Inventory.Repositories.Inventory;

namespace Storet.Modules.Inventory.Setup;

public static class InventorySetup
{
	public static IServiceCollection AddInventoryModule (this IServiceCollection services, string connectionString)
	{
		services.AddDbContext<StoretInventoryDbContext> (
			options => options.UseNpgsql (
							connectionString,
							opt =>
							{
								opt.MapEnum<Status> (schemaName: "inventory", enumName: "statuses");
								opt.MapEnum<MovementDirection> (schemaName: "inventory", enumName: "directions");
								opt.MapEnum<MovementSource> (schemaName: "inventory", enumName: "sources");
							}
						)
						.EnableSensitiveDataLogging()
		);
		
		services.AddScoped<IInventoryRepository, InventoryRepository>();
		
		return services;
	}
}