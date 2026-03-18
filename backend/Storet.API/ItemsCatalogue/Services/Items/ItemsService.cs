using AutoMapper;
using Storet.API.Core.Exceptions;
using Storet.API.Core.Utils;
using Storet.API.ItemsCatalogue.Contracts.Items;
using Storet.API.ItemsCatalogue.Contracts.ItemsCompositions;
using Storet.API.ItemsCatalogue.Models;
using Storet.API.ItemsCatalogue.Repositories.Categories;
using Storet.API.ItemsCatalogue.Repositories.Items;
using Storet.API.ItemsCatalogue.Repositories.ItemsCategories;
using Storet.API.ItemsCatalogue.Repositories.ItemsCompositions;

namespace Storet.API.ItemsCatalogue.Services.Items;

public class ItemsService : IItemsService
{
	private readonly ICategoriesRepository categoriesRepository;
	private readonly IItemsRepository itemsRepository;
	private readonly IItemsCategoriesRepository itemsCategoriesRepository;
	private readonly IItemsCompositionRepository itemsCompositionRepository;
	private readonly IMapper mapper;
	
	public ItemsService (IItemsRepository itemsRepository, ICategoriesRepository categoriesRepository, IItemsCategoriesRepository itemsCategoriesRepository, IItemsCompositionRepository itemsCompositionRepository, IMapper mapper)
	{
		this.itemsRepository = itemsRepository;
		this.categoriesRepository = categoriesRepository;
		this.itemsCategoriesRepository = itemsCategoriesRepository;
		this.itemsCompositionRepository = itemsCompositionRepository;
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
	
	public async Task<IEnumerable<ItemResponse>> GetAllComponentsAsync ()
	{
		var result = await itemsRepository.GetAllComponentsAsync();
		return result.Select (mapper.Map<Item, ItemResponse>);
	}
	
	public async Task<ItemResponseDetails?> GetOneAsync (Guid key)
	{
		var item = await itemsRepository.GetOneAsync (key);
		if (item == null)
			return null;
			
		return mapper.Map<Item, ItemResponseDetails> (item);
	}
	
	/// <summary>Separates the components to existsing and new, checks if provided item ids exist in the database and inserts new components as items</summary>
	/// <param name = "components">The list of components to be sorted and checked</param>
	/// <returns>A dictionnary that maps existing item ids to their quantity and unit and a list of item component models to be created in the database and added to the item</returns>
	/// <exception cref = "EntityNotFoundException">Thrown if <paramref name = "providedExistingItemIds"/> contains one or many non existing item ids.</exception>
	/// <exception cref = "ArgumentException">Thrown if <paramref name = "newItemsToBeCreated"/> contains an invalid model to create the item</exception>
	private async Task<(Dictionary<Guid, short> providedExistingItemIds, IEnumerable<ItemCompositionRequest> newItemsToBeCreated)> CheckIfComponentsExist (IEnumerable<ItemCompositionRequest>? components)
	{
		Dictionary<Guid, short> providedExistingItemIds = [];
		IEnumerable<ItemCompositionRequest> newItemsToBeCreated = [];
		
		if (components != null && components.Any())
		{
			providedExistingItemIds = components.Where (c => c.Id != null)
												.ToDictionary (
													c => c.Id!.Value,
													c => c.Quantity
												);
			
			if (providedExistingItemIds.Keys.Count > 0)
			{
				var itemsExistInDatabase = await itemsRepository.AllExistAsync (providedExistingItemIds.Keys.ToList());
				if (!itemsExistInDatabase)
					throw new EntityNotFoundException (nameof (Item), providedExistingItemIds.Keys);
			}
			
			newItemsToBeCreated = components.Where (c => c.Id == null);
			var invalidItemsExist = newItemsToBeCreated.Any (
															i => string.IsNullOrWhiteSpace (i.Name)
																|| i.Quantity < 1
														);
			if (invalidItemsExist)
				throw new ArgumentException ("Non existent item component must have all required fields in order to be added correctly");
		}

		return (providedExistingItemIds, newItemsToBeCreated);
	}
	
	private async Task<(IEnumerable<ItemCompositionRequest>? oldComposition, Dictionary<Guid, short> providedExistingItemIds, IEnumerable<ItemCompositionRequest> newItemsToBeCreated)> CheckIfComponentsExist (Guid itemId, IEnumerable<ItemCompositionRequest>? components)
	{
		var rawComponents = await itemsCompositionRepository.GetAllForItemAsync (itemId);
		
		var oldComponents = rawComponents?.Select (
										c => new ItemCompositionRequest
											{
												Id = c.ComponentItemId,
												Quantity = c.Quantity
											}
										);
		
		var (providedExistingItemIds, newItemsToBeCreated) = await CheckIfComponentsExist (components);
		return (oldComponents, providedExistingItemIds, newItemsToBeCreated);
	}
	
	private async Task InsertItemCategories (Guid insertedItemId, ItemInsertRequest model)
	{
		var itemsCategories = model.Categories
									.Select (
										cId => new ItemCategory
										{
											ItemId = insertedItemId,
											CategoryId = cId
										}
									);
		var insertedCategories = await itemsCategoriesRepository.BulkInsertAsync (itemsCategories);
	}
	
	private async Task<IEnumerable<ItemComposition>> MergeComponentsForItem (Guid itemId, Dictionary<Guid, short> providedExistingItemIds, IEnumerable<ItemCompositionRequest> newItemsToBeCreated, float parentItemQuantity, Unit parentItemUnit)
	{
		IEnumerable<ItemComposition> itemsCompositionsToBeAdded = [];
		if (newItemsToBeCreated.Any())
		{
			var itemsModels = newItemsToBeCreated.Select (
								i => new Item
									{
										Name = i.Name!,
										Description = i.Description,
										Quantity = parentItemQuantity / i.Quantity,
										Unit = parentItemUnit,
										IsComponent = true
									}
								);
			var created = await itemsRepository.BulkInsertAsync (itemsModels);
			itemsCompositionsToBeAdded = created.Join (
													newItemsToBeCreated,
													c => c.Name,
													i => i.Name,
													(created, composition) => new ItemComposition
													{
														ParentItemId = itemId,
														ComponentItemId = created.Id,
														Quantity = composition.Quantity
													}
												);
		}
		
		if (providedExistingItemIds.Count > 0)
			itemsCompositionsToBeAdded = itemsCompositionsToBeAdded.Concat (
											providedExistingItemIds.Select (
												kv => new ItemComposition
												{
													ParentItemId = itemId,
													ComponentItemId = kv.Key,
													Quantity = kv.Value
												}
											)
										);
										
		return itemsCompositionsToBeAdded;
	}
	
	private async Task InsertItemComponents (Item item, Dictionary<Guid, short> providedExistingItemIds, IEnumerable<ItemCompositionRequest> newItemsToBeCreated)
	{
		var itemsCompositionsToBeAdded = await MergeComponentsForItem (item.Id, providedExistingItemIds, newItemsToBeCreated, item.Quantity, item.Unit);
		
		if (itemsCompositionsToBeAdded.Any())
			await itemsCompositionRepository.BulkInsertAsync (itemsCompositionsToBeAdded);
	}
	
	private static (IEnumerable<Guid> removedIds, Dictionary<Guid, short> updatedValues) GetComponentsDiffForItem (IEnumerable<ItemCompositionRequest>? oldComposition, Dictionary<Guid, short> providedExistingItemIds)
	{
		IEnumerable<Guid> removedIds = [];
		Dictionary<Guid, short> updatedValues = [];
		
		if (oldComposition == null || !oldComposition.Any())
			return (removedIds, updatedValues);
		
		removedIds = oldComposition.Select (c => c.Id!.Value)
									.Except (providedExistingItemIds.Keys)
									.ToList();
		foreach (var id in removedIds)
			providedExistingItemIds.Remove (id);
		
		var oldCompositionDict = oldComposition.ToDictionary (
									c => c.Id!.Value,
									c => c.Quantity
								);
		
		updatedValues = providedExistingItemIds.Where (
							pair => oldCompositionDict.ContainsKey (pair.Key)
									&& oldCompositionDict.GetValueOrDefault(pair.Key)! != pair.Value
						)
						.ToDictionary (
							p => p.Key,
							p => p.Value
						);
		
		foreach (var id in updatedValues.Keys)
			providedExistingItemIds.Remove (id);
		
		return (removedIds, updatedValues);
	}
	
	private async Task UpdateComponentsForItem (Item item, IEnumerable<ItemCompositionRequest>? oldComponents, Dictionary<Guid, short> providedExistingItemIds, IEnumerable<ItemCompositionRequest> newItemsToBeCreated)
	{
		var (removedItemComponents, updatedValues) = GetComponentsDiffForItem (oldComponents, providedExistingItemIds);
		if (removedItemComponents.Any())
		{
			await itemsCompositionRepository.BulkDeleteForItemAsync (item.Id, removedItemComponents);
			var notUsedItems = await itemsCompositionRepository.GetNotUsedByAnyAsync (removedItemComponents);
			if (notUsedItems.Any())
				await itemsRepository.BulkDeleteAsync (notUsedItems);
		}
		if (updatedValues.Count != 0)
			foreach (var modification in updatedValues)
				await itemsCompositionRepository.UpdateAsync (item.Id, modification.Key, modification.Value);
		
		if (providedExistingItemIds.Count > 0)
			await InsertItemComponents (item, providedExistingItemIds, newItemsToBeCreated);
	}
	
	public async Task<ItemResponseDetails?> InsertAsync (ItemInsertRequest model)
	{
		if (model.Categories.Count < 1)
			throw new ArgumentException ("Item must be created with at least 1 category");
		if (model.Quantity <= 0)
			throw new ArgumentException ("Item must have a positive quantity");
			
		var allCategoriesExist = await categoriesRepository.AllExistAsync (model.Categories);
		if (!allCategoriesExist)
			throw new EntityNotFoundException (nameof (Category), model.Categories);
		
		var (providedExistingItemIds, newItemsToBeCreated) = await CheckIfComponentsExist (model.Components);
		
		var insertedItem = await itemsRepository.InsertAsync (mapper.Map<ItemInsertRequest, Item> (model));
		if (insertedItem == null)
			return null;
		
		await InsertItemCategories (insertedItem.Id, model);
		
		await InsertItemComponents (insertedItem, providedExistingItemIds, newItemsToBeCreated);
		
		var fullEntity = await itemsRepository.GetOneAsync (insertedItem.Id);
		return mapper.Map<Item, ItemResponseDetails> (fullEntity!);
	}
	
	private async Task<(IEnumerable<int> deletedCategoryIds, IEnumerable<int> addedCategoryIds)> CheckIfCategoriesExistBeforeUpdatingItem (Guid key, ItemUpdateRequest model)
	{
		IEnumerable<int> deletedCategoryIds = [], addedCategoryIds = [];
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

		return (deletedCategoryIds, addedCategoryIds);
	}
	
	private async Task UpdateCategoriesForItem (Guid key, IEnumerable<int> deletedCategoryIds, IEnumerable<int> addedCategoryIds)
	{
		if (deletedCategoryIds.Any())
			await itemsCategoriesRepository.BulkDeleteForItemAsync (key, deletedCategoryIds);
		if (addedCategoryIds.Any())
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
	}
	
	public async Task<ItemResponseDetails?> UpdateAsync (Guid key, ItemUpdateRequest model)
	{
		var itemExists = await itemsRepository.ExistsAsync (key);
		if (!itemExists)
			return null;
		
		var (deletedCategoryIds, addedCategoryIds) = await CheckIfCategoriesExistBeforeUpdatingItem (key, model);
		
		var (oldComponents, providedExistingItemIds, newItemsToBeCreated) = await CheckIfComponentsExist (key, model.Components);
		
		var updatedItem = await itemsRepository.UpdateAsync (key, mapper.Map<ItemUpdateRequest, Item> (model));
		if (updatedItem == null)
			return null;
		
		await UpdateCategoriesForItem (key, deletedCategoryIds, addedCategoryIds);
		
		await UpdateComponentsForItem (updatedItem, oldComponents, providedExistingItemIds, newItemsToBeCreated);
		
		var finalEntity = await itemsRepository.GetOneAsync (key);
		return mapper.Map<Item, ItemResponseDetails> (finalEntity!);
	}
	
	public async Task<bool> DeleteAsync (Guid key)
	{
		try
		{
			await itemsCategoriesRepository.DeleteAllForItemAsync (key);
			await itemsCompositionRepository.DeleteAllForItemAsync (key);
			return await itemsRepository.DeleteAsync (key);
		}
		catch (Exception)
		{
			return false;
		}
	}
}