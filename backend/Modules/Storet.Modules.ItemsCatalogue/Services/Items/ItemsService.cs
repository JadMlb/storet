using AutoMapper;
using MediatR;
using Storet.Core.Authorization;
using Storet.Core.Exceptions;
using Storet.Core.Utils;
using Storet.Modules.ItemsCatalogue.Contracts.Items;
using Storet.Modules.ItemsCatalogue.Contracts.ItemsCompositions;
using Storet.Modules.ItemsCatalogue.Events;
using Storet.Modules.ItemsCatalogue.Models;
using Storet.Modules.ItemsCatalogue.Repositories.Categories;
using Storet.Modules.ItemsCatalogue.Repositories.Items;
using Storet.Modules.ItemsCatalogue.Repositories.ItemsCategories;
using Storet.Modules.ItemsCatalogue.Repositories.ItemsCompositions;

namespace Storet.Modules.ItemsCatalogue.Services.Items;

public class ItemsService : IItemsService
{
	private readonly ICategoriesRepository categoriesRepository;
	private readonly IItemsRepository itemsRepository;
	private readonly IItemsCategoriesRepository itemsCategoriesRepository;
	private readonly IItemsCompositionRepository itemsCompositionRepository;
	private readonly ICurrentUser user;
	private readonly IMapper mapper;
	private readonly IMediator mediator;
	
	public ItemsService (IItemsRepository itemsRepository, ICategoriesRepository categoriesRepository, IItemsCategoriesRepository itemsCategoriesRepository, IItemsCompositionRepository itemsCompositionRepository, ICurrentUser user, IMapper mapper, IMediator mediator)
	{
		this.itemsRepository = itemsRepository;
		this.categoriesRepository = categoriesRepository;
		this.itemsCategoriesRepository = itemsCategoriesRepository;
		this.itemsCompositionRepository = itemsCompositionRepository;
		this.user = user;
		this.mapper = mapper;
		this.mediator = mediator;
	}
	
	public async Task<PaginatedResponse<ItemResponse, string>> GetAllAsync (Query<string> query)
	{
		if (query.PageSize < 1)
			throw new MalformedRequestException ("Invalid page size for query");
		
		var data = await itemsRepository.GetAllAsync (query, user.Id);
		var previousKey = await itemsRepository.GetPreviousKeyAsync (query, user.Id);
		
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
		var result = await itemsRepository.GetAllComponentsAsync (user.Id);
		return result.Select (mapper.Map<Item, ItemResponse>);
	}
	
	public async Task<ItemResponseDetails?> GetOneAsync (Guid key)
	{
		var item = await itemsRepository.GetOneAsync (key, user.Id);
		if (item == null)
			return null;
			
		return mapper.Map<Item, ItemResponseDetails> (item);
	}
	
	public async Task<Dictionary<Guid, List<Guid>>> GetComponentIdsForItemsAsync (IEnumerable<Guid> itemsIds)
	{
		return await itemsCompositionRepository.GetComponentIdsForItemsAsync (itemsIds, user.Id);
	}
	
	public async Task<Dictionary<Guid, ItemResponseWithUnit>> GetAllFromListWithUnitAsync (IEnumerable<Guid> ids)
	{
		var items = await itemsRepository.GetAllFromListAsync (user.Id, ids);
		return items.ToDictionary (i => i.Id, mapper.Map<Item, ItemResponseWithUnit>);
	}

	public async Task<bool> ExistsAsync (Guid itemId)
	{
		return await itemsRepository.ExistsAsync (itemId, user.Id);
	}

	public async Task<bool> AllExistAsync (IEnumerable<Guid> itemsIds)
	{
		return await itemsRepository.AllExistAsync (user.Id, itemsIds);
	}
	
	/// <summary>Separates the components to existsing and new, checks if provided item ids exist in the database and inserts new components as items</summary>
	/// <param name = "components">The list of components to be sorted and checked</param>
	/// <returns>A dictionnary that maps existing item ids to their quantity and unit and a list of item component models to be created in the database and added to the item</returns>
	/// <exception cref = "EntityNotFoundException">Thrown if <paramref name = "providedExistingItemIds"/> contains one or many non existing item ids.</exception>
	/// <exception cref = "MalformedRequestException">Thrown if <paramref name = "newItemsToBeCreated"/> contains an invalid model to create the item</exception>
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
				var itemsExistInDatabase = await itemsRepository.AllExistAsync (user.Id, providedExistingItemIds.Keys.ToList());
				if (!itemsExistInDatabase)
					throw new EntityNotFoundException (nameof (Item), providedExistingItemIds.Keys);
			}
			
			newItemsToBeCreated = components.Where (c => c.Id == null);
			var invalidItemsExist = newItemsToBeCreated.Any (
															i => string.IsNullOrWhiteSpace (i.Name)
																|| i.Quantity < 1
														);
			if (invalidItemsExist)
				throw new MalformedRequestException ("Non existent item component must have all required fields in order to be added correctly");
		}

		return (providedExistingItemIds, newItemsToBeCreated);
	}
	
	private async Task InsertItemCategories (Guid insertedItemId, ItemInsertRequest model)
	{
		var itemsCategories = model.Categories
									.Select (
										cId => new ItemCategory
										{
											ItemId = insertedItemId,
											CategoryId = cId,
											UserId = user.Id
										}
									)
									.ToList();
		var insertedCategories = await itemsCategoriesRepository.BulkInsertAsync (itemsCategories);
	}
	
	private async Task<IEnumerable<ItemComposition>> MergeComponentsForItem (Guid itemId, Dictionary<Guid, short> providedExistingItemIds, IEnumerable<ItemCompositionRequest> newItemsToBeCreated, float parentItemQuantity, Models.Unit parentItemUnit)
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
										IsComponent = true,
										UserId = user.Id
									}
								)
								.ToList();
			var created = await itemsRepository.BulkInsertAsync (itemsModels);
			await mediator.Publish (new ItemsCreatedEvent (created.Select (i => i.Id), user.Id));
			itemsCompositionsToBeAdded = created.Join (
													newItemsToBeCreated,
													c => c.Name,
													i => i.Name,
													(created, composition) => new ItemComposition
													{
														ParentItemId = itemId,
														ComponentItemId = created.Id,
														Quantity = composition.Quantity,
														UserId = user.Id
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
													Quantity = kv.Value,
													UserId = user.Id
												}
											)
										);
										
		return itemsCompositionsToBeAdded.ToList();
	}
	
	private async Task InsertItemComponents (Item item, Dictionary<Guid, short> providedExistingItemIds, IEnumerable<ItemCompositionRequest> newItemsToBeCreated)
	{
		var itemsCompositionsToBeAdded = await MergeComponentsForItem (item.Id, providedExistingItemIds, newItemsToBeCreated, item.Quantity, item.Unit);
		
		if (itemsCompositionsToBeAdded.Any())
			await itemsCompositionRepository.BulkInsertAsync (itemsCompositionsToBeAdded);
	}
	
	private async Task DeleteItemComponents (IEnumerable<Guid> toBeDeleted)
	{
		if (!toBeDeleted.Any())
			return;
			
		var notUsedItems = await itemsCompositionRepository.GetNotUsedByAnyAsync (toBeDeleted, user.Id);
		if (!notUsedItems.Any())
			return;
		
		await itemsRepository.BulkDeleteAsync (notUsedItems, user.Id);
		await mediator.Publish (new ItemsDeletedEvent (notUsedItems, user.Id));
	}
	
	public async Task<ItemResponseDetails?> InsertAsync (ItemInsertRequest model)
	{
		if (model.Categories.Count < 1)
			throw new BusinessRuleException ("Item must be created with at least 1 category");
		if (model.Quantity <= 0)
			throw new MalformedRequestException ("Item must have a positive quantity");
			
		var allCategoriesExist = await categoriesRepository.AllExistAsync (user.Id, model.Categories);
		if (!allCategoriesExist)
			throw new EntityNotFoundException (nameof (Category), model.Categories);
		
		var (providedExistingItemIds, newItemsToBeCreated) = await CheckIfComponentsExist (model.Components);
		
		var insertedItem = await itemsRepository.InsertAsync (mapper.Map<ItemInsertRequest, Item> (model));
		if (insertedItem == null)
			return null;
		
		await InsertItemCategories (insertedItem.Id, model);
		
		await InsertItemComponents (insertedItem, providedExistingItemIds, newItemsToBeCreated);
		
		if (model.Components == null || model.Components.Count == 0)
			await mediator.Publish (new ItemsCreatedEvent ([insertedItem.Id], user.Id));
		var fullEntity = await itemsRepository.GetOneAsync (insertedItem.Id, user.Id);
		return mapper.Map<Item, ItemResponseDetails> (fullEntity!);
	}
	
	private async Task<(IEnumerable<int> deletedCategoryIds, IEnumerable<int> addedCategoryIds)> CheckIfCategoriesExistBeforeUpdatingItem (Guid key, ItemUpdateRequest model)
	{
		IEnumerable<int> deletedCategoryIds = [], addedCategoryIds = [];
		if (model.Categories != null && model.Categories.Count > 0)
		{
			var newCategoriesListExists = await categoriesRepository.AllExistAsync (user.Id, model.Categories);
			
			if (!newCategoriesListExists)
				throw new EntityNotFoundException (nameof (Category), model.Categories);
				
			// get the dif changes
			var oldCategoriesRelationships = await itemsCategoriesRepository.GetAllForItemAsync (key, user.Id);
			var oldCategories = oldCategoriesRelationships.Select (r => r.CategoryId);
			deletedCategoryIds = oldCategories.Except (model.Categories);
			addedCategoryIds = model.Categories.Except (oldCategories);
		}

		return (deletedCategoryIds, addedCategoryIds);
	}
	
	private async Task UpdateCategoriesForItem (Guid key, IEnumerable<int> deletedCategoryIds, IEnumerable<int> addedCategoryIds)
	{
		if (deletedCategoryIds.Any())
			await itemsCategoriesRepository.BulkDeleteForItemAsync (key, user.Id, deletedCategoryIds);
		if (addedCategoryIds.Any())
		{
			var toBeInserted = addedCategoryIds.Select (
				cId => new ItemCategory
				{
					CategoryId = cId,
					ItemId = key,
					UserId = user.Id
				}
			)
			.ToList();
			await itemsCategoriesRepository.BulkInsertAsync (toBeInserted);
		}
	}
	
	public async Task<ItemResponseDetails?> UpdateAsync (Guid key, ItemUpdateRequest model)
	{
		var itemExists = await itemsRepository.ExistsAsync (key, user.Id);
		if (!itemExists)
			return null;
		
		var (deletedCategoryIds, addedCategoryIds) = await CheckIfCategoriesExistBeforeUpdatingItem (key, model);
		
		var updatedItem = await itemsRepository.UpdateAsync (key, user.Id, mapper.Map<ItemUpdateRequest, Item> (model));
		if (updatedItem == null)
			return null;
		
		await UpdateCategoriesForItem (key, deletedCategoryIds, addedCategoryIds);
		
		var finalEntity = await itemsRepository.GetOneAsync (key, user.Id);
		return mapper.Map<Item, ItemResponseDetails> (finalEntity!);
	}
	
	public async Task<bool> DeleteAsync (Guid key)
	{
		try
		{
			await itemsCategoriesRepository.DeleteAllForItemAsync (key, user.Id);
			var itemsInDeletedRelations = await itemsCompositionRepository.DeleteAllForItemAsync (key, user.Id);
			await DeleteItemComponents (itemsInDeletedRelations);
			return await itemsRepository.DeleteAsync (key, user.Id);
		}
		catch (Exception)
		{
			return false;
		}
	}
}