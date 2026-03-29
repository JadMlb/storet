using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Storet.Core.Mappers;
using Storet.Modules.ItemsCatalogue.Contracts.Categories;
using Storet.Modules.ItemsCatalogue.Contracts.Items;
using Storet.Modules.ItemsCatalogue.Data;
using Storet.Modules.ItemsCatalogue.Mappers;
using Storet.Modules.ItemsCatalogue.Models;
using Storet.Modules.ItemsCatalogue.Repositories.Categories;
using Storet.Modules.ItemsCatalogue.Repositories.Items;
using Storet.Modules.ItemsCatalogue.Repositories.ItemsCategories;
using Storet.Modules.ItemsCatalogue.Repositories.ItemsCompositions;
using Storet.Modules.ItemsCatalogue.Services.Categories;
using Storet.Modules.ItemsCatalogue.Services.Items;

namespace Storet.Modules.ItemsCatalogue.Setup;

public static class ItemsCatalogueSetup
{
	public static IServiceCollection AddItemsCatalogueModule (this IServiceCollection services, string connectionString)
	{
		services.AddDbContext<StoretItemsCatalogueDbContext> (
			options => options.UseNpgsql (
							connectionString,
							opt => opt.MapEnum<Unit> (schemaName: "items_catalogue", enumName: "units")
						)
						.EnableSensitiveDataLogging()
		);
		
		services.AddScoped (typeof (CurrentUserResolver<,>));
		services.AddScoped<CurrentUserResolver<CategoryInsertRequest, Category>>();
		services.AddScoped<CurrentUserResolver<CategoryUpdateRequest, Category>>();
		services.AddScoped<CurrentUserResolver<ItemInsertRequest, Item>>();
		services.AddScoped<CurrentUserResolver<ItemUpdateRequest, Item>>();
		services.AddAutoMapper (
			(sp, cfg) =>
			{
				cfg.ConstructServicesUsing (type => ActivatorUtilities.CreateInstance (sp, type));
				cfg.AddProfile<MappingProfile>();
			},
			Assembly.GetExecutingAssembly()
		);
		
		services.AddScoped<ICategoriesRepository, CategoriesRepository>();
		services.AddScoped<ICategoriesService, CategoriesService>();
		
		services.AddScoped<IItemsCompositionRepository, ItemsCompositionsRepository>();
		
		services.AddScoped<IItemsCategoriesRepository, ItemsCategoriesRepository>();
		services.AddScoped<IItemsRepository, ItemsRepository>();
		services.AddScoped<IItemsService, ItemsService>();
		
		return services;
	}
}