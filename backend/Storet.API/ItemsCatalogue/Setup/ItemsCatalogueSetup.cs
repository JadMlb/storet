using Microsoft.EntityFrameworkCore;
using Storet.API.ItemsCatalogue.Data;
using Storet.API.ItemsCatalogue.Mappers;
using Storet.API.ItemsCatalogue.Models;
using Storet.API.ItemsCatalogue.Repositories.Categories;
using Storet.API.ItemsCatalogue.Repositories.Items;
using Storet.API.ItemsCatalogue.Repositories.ItemsCategories;
using Storet.API.ItemsCatalogue.Repositories.ItemsCompositions;
using Storet.API.ItemsCatalogue.Services.Categories;
using Storet.API.ItemsCatalogue.Services.Items;

namespace Storet.API.ItemsCatalogue.Setup;

public static class ItemsCatalogueSetup
{
	public static IServiceCollection AddItemsCatalogueModule (this IServiceCollection services, string connectionString)
	{
		services.AddDbContext<StoretItemsCatalogueDbContext> (
			options => options.UseNpgsql (
							connectionString,
							opt => opt.MapEnum<Unit> (schemaName: "items_catalogue", enumName: "units")
						)
		);
		
		services.AddAutoMapper (cfg => cfg.AddProfile<MappingProfile>());
		
		services.AddScoped<ICategoriesRepository, CategoriesRepository>();
		services.AddScoped<ICategoriesService, CategoriesService>();
		
		services.AddScoped<IItemsCompositionRepository, ItemsCompositionsRepository>();
		
		services.AddScoped<IItemsCategoriesRepository, ItemsCategoriesRepository>();
		services.AddScoped<IItemsRepository, ItemsRepository>();
		services.AddScoped<IItemsService, ItemsService>();
		
		return services;
	}
}