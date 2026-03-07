using AutoMapper;
using Storet.API.Core.Exceptions;
using Storet.API.Core.Utils;
using Storet.API.ItemsCatalogue.Contracts.Items;
using Storet.API.ItemsCatalogue.Models;
using Storet.API.ItemsCatalogue.Repositories.Categories;
using Storet.API.ItemsCatalogue.Repositories.Items;
using Storet.API.ItemsCatalogue.Repositories.ItemsCategories;

namespace Storet.API.ItemsCatalogue.Services.Items;

public class ItemsService : IItemsService
{
	private readonly ICategoriesRepository categoriesRepository;
	private readonly IItemsRepository itemsRepository;
	private readonly IItemsCategoriesRepository itemsCategoriesRepository;
	private readonly IMapper mapper;
	
	public ItemsService (IItemsRepository itemsRepository, ICategoriesRepository categoriesRepository, IItemsCategoriesRepository itemsCategoriesRepository, IMapper mapper)
	{
		this.itemsRepository = itemsRepository;
		this.categoriesRepository = categoriesRepository;
		this.itemsCategoriesRepository = itemsCategoriesRepository;
		this.mapper = mapper;
	}
	
	public async Task<PaginatedResponse<ItemResponse, string>> GetAllAsync (Query<string> query)
	{
		if (query.PageSize < 1)
			throw new ArgumentException ("Invalid page size for query");
		
		var data = await itemsRepository.GetAllAsync (query);
		var previousKey = await itemsRepository.GetPreviousKeyAsync (query);
		
		var response = new PaginatedResponse<ItemResponse, string>();
		if (data.Count() > query.PageSize)
		{
			response.Next = data.Last().Name;
			response.Data = data.Take(query.PageSize).Select(mapper.Map<Item, ItemResponse>).ToList();
		}
		else
			response.Data = data.Select(mapper.Map<Item, ItemResponse>).ToList();
			
		response.Previous = previousKey;
		
		return response;
	}
	
	public async Task<ItemResponseWithCategories?> GetOneAsync (Guid key)
	{
		var item = await itemsRepository.GetOneAsync (key);
		if (item == null)
			return null;
			
		return mapper.Map<Item, ItemResponseWithCategories> (item);
	}
	
	public async Task<ItemResponseWithCategories?> InsertAsync (ItemInsertRequest model)
	{
		if (model.Categories.Count < 1)
			throw new ArgumentException ("Item must be created with at least 1 category");
			
		var allCategoriesExist = await categoriesRepository.AllExistAsync (model.Categories);
		if (!allCategoriesExist)
			throw new EntityNotFoundException (nameof (Category), model.Categories);
		
		var insertedItem = await itemsRepository.InsertAsync (mapper.Map<ItemInsertRequest, Item> (model));
		if (insertedItem == null)
			return null;
		
		var itemsCategories = model.Categories
									.Select (
										cId => new ItemCategory
										{
											ItemId = insertedItem.Id,
											CategoryId = cId
										}
									);
		var insertedCategories = await itemsCategoriesRepository.BulkInsertAsync (itemsCategories);
		
		var fullEntity = await itemsRepository.GetOneAsync (insertedItem.Id);
		return mapper.Map<Item, ItemResponseWithCategories> (fullEntity!);
	}
	
	public async Task<ItemResponseWithCategories?> UpdateAsync (Guid key, ItemUpdateRequest model)
	{
		var itemExists = await itemsRepository.ExistsAsync (key);
		if (!itemExists)
			return null;
		
		IEnumerable<int>? deletedCategoryIds = null, addedCategoryIds = null;
		if (model.Categories != null && model.Categories.Count > 0)
		{
			var newCategoriesListExists = await categoriesRepository.AllExistAsync (model.Categories);
			
			if (!newCategoriesListExists)
				throw new EntityNotFoundException (nameof (Category), model.Categories);
				
			// get the dif changes
			var oldCategoriesRelationships = await itemsCategoriesRepository.GetAllForItemAsync (key);
			var oldCategories = oldCategoriesRelationships.Select (r => r.CategoryId);
			deletedCategoryIds = oldCategories.Except (model.Categories);
			addedCategoryIds = model.Categories.Except (oldCategories);
		}
		
		var updatedItem = await itemsRepository.UpdateAsync (key, mapper.Map<ItemUpdateRequest, Item> (model));
		if (updatedItem == null)
			return null;
			
		if (deletedCategoryIds != null && deletedCategoryIds.Any())
			await itemsCategoriesRepository.BulkDeleteForItemAsync (key, deletedCategoryIds);
		if (addedCategoryIds != null && addedCategoryIds.Any())
		{
			var toBeInserted = addedCategoryIds.Select (
				cId => new ItemCategory
				{
					CategoryId = cId,
					ItemId = key
				}
			);
			await itemsCategoriesRepository.BulkInsertAsync (toBeInserted);
		}
		
		var finalEntity = await itemsRepository.GetOneAsync (key);
		return mapper.Map<Item, ItemResponseWithCategories> (finalEntity!);
	}
	
	public async Task<bool> DeleteAsync (Guid key)
	{
		try
		{
			await itemsCategoriesRepository.DeleteAllForItemAsync (key);
			return await itemsRepository.DeleteAsync (key);
		}
		catch (Exception)
		{
			return false;
		}
	}
}