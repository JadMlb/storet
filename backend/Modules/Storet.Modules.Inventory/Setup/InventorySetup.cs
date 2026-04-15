using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Storet.Core.Mappers;
using Storet.Modules.Inventory.Contracts.Inventory;
using Storet.Modules.Inventory.Contracts.InventoryMovement;
using Storet.Modules.Inventory.Contracts.StorageLocation;
using Storet.Modules.Inventory.Data;
using Storet.Modules.Inventory.Mappers;
using Storet.Modules.Inventory.Models;
using Storet.Modules.Inventory.Repositories.Inventory;
using Storet.Modules.Inventory.Repositories.InventoryMovementItems;
using Storet.Modules.Inventory.Repositories.InventoryMovements;
using Storet.Modules.Inventory.Repositories.StorageLocations;
using Storet.Modules.Inventory.Services.Inventory;
using Storet.Modules.Inventory.Services.InventoryMovements;
using Storet.Modules.Inventory.Services.StorageLocations;

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
		
		services.AddScoped (typeof (CurrentUserResolver<,>));
		services.AddScoped<CurrentUserResolver<InventoryInsertRequest, Models.Inventory>>();
		services.AddScoped<CurrentUserResolver<InventoryUpdateRequest, Models.Inventory>>();
		services.AddScoped<CurrentUserResolver<StorageLocationInsertRequest, StorageLocation>>();
		services.AddScoped<CurrentUserResolver<StorageLocationUpdateRequest, StorageLocation>>();
		services.AddScoped<CurrentUserResolver<InventoryModificationRequest, InventoryMovement>>();
		services.AddAutoMapper (
			(sp, cfg) =>
			{
				cfg.ConstructServicesUsing (type => ActivatorUtilities.CreateInstance (sp, type));
				cfg.AddProfile<MappingProfile>();
			},
			Assembly.GetExecutingAssembly()
		);
		
		services.AddScoped<IInventoryRepository, InventoryRepository>();
		services.AddScoped<IInventoryService, InventoryService>();
		
		services.AddScoped<IStorageLocationsRepository, StorageLocationsRepository>();
		services.AddScoped<IStorageLocationsService, StorageLocationsService>();
		
		services.AddScoped<IInventoryMovementsRepository, InventoryMovementsRepository>();
		services.AddScoped<IInventoryMovementItemsRepository, InventoryMovementItemsRepository>();
		services.AddScoped<IInventoryMovementsService, InventoryMovementsService>();
		
		return services;
	}
}