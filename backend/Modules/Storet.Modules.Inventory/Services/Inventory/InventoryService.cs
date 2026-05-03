using AutoMapper;
using Storet.Core.Authorization;
using Storet.Core.Exceptions;
using Storet.Modules.Inventory.Contracts.Inventory;
using Storet.Modules.Inventory.Models;
using Storet.Modules.Inventory.Queries;
using Storet.Modules.Inventory.Repositories.Inventory;
using Storet.Modules.ItemsCatalogue.Models;
using Storet.Modules.ItemsCatalogue.Services.Items;

namespace Storet.Modules.Inventory.Services.Inventory;

public class InventoryService : IInventoryService
{
	private readonly IInventoryRepository inventoryRepository;
	private readonly IItemsService itemsService;
	private readonly ICurrentUser user;
	private readonly IMapper mapper;
	
	public InventoryService (IInventoryRepository inventoryRepository, IItemsService itemsService, ICurrentUser user, IMapper mapper)
	{
		this.inventoryRepository = inventoryRepository;
		this.itemsService = itemsService;
		this.user = user;
		this.mapper = mapper;
	}
	
	public async Task<Dictionary<Guid, IEnumerable<InventoryResponse>>> GetAllAsync (InventoryFilterQuery query)
	{
		if (string.IsNullOrWhiteSpace (query.Items))
			throw new ArgumentException ("Cannot fetch inventories for empty items list");
		
		var mayBeItemsIdsStrings = query.Items.Split (",", StringSplitOptions.RemoveEmptyEntries);
		List<Guid> itemsIds = [];
		foreach (var mayBeGuid in mayBeItemsIdsStrings)
		{
			if (!Guid.TryParse (mayBeGuid, out var guid))
				throw new ArgumentException ("One or more provided ID is not a valid Guid");
			itemsIds.Add (guid);
		}
		
		var componentsByItemId = await itemsService.GetComponentIdsForItemsAsync (itemsIds);
		
		var componentsIdsToFetchInventoriesFor = componentsByItemId.SelectMany(kv => kv.Value.Count == 0 ? [kv.Key] : kv.Value).ToList();
		if (componentsIdsToFetchInventoriesFor.Count == 0)
			return [];
		
		var inventoriesByComponentId = await inventoryRepository.GetAllFromListAsync (user.Id, componentsIdsToFetchInventoriesFor);
		
		Dictionary<Guid, IEnumerable<InventoryResponse>> result = [];
		foreach (var parentItemId in itemsIds)
		{
			var componentsForParent = componentsByItemId.GetValueOrDefault (parentItemId, []);
			if (componentsForParent.Count == 0)
				componentsForParent = [parentItemId];

			var componentInventories = inventoriesByComponentId.Where (kv => componentsForParent.Contains (kv.Key))
																.Select (kv => mapper.Map<Models.Inventory, InventoryResponse> (kv.Value))
																.ToList();
			result.Add (parentItemId, componentInventories);
		}
		
		return result;
	}

	public async Task<IEnumerable<InventoryResponse>> GetOneAsync (Guid itemId)
	{
		var componentsIdsByParentId = await itemsService.GetComponentIdsForItemsAsync ([itemId]);
		var componentsIds = componentsIdsByParentId.GetValueOrDefault (itemId, []);
		if (componentsIds.Count == 0)
			componentsIds = [itemId];
		var inventories = await inventoryRepository.GetAllFromListAsync (user.Id, componentsIds);
		return inventories.Select (kv => mapper.Map<Models.Inventory, InventoryResponse> (kv.Value))
							.ToList();
	}
	
	private Status GetStatusFromQuantity (float quantity, float minQuantity = 0, float? maxQuantity = null)
	{
		// [0, ~]
		// [a, ~]
		// - x < a -> empty not accepted
		// - x = a -> empty accepted
		// - x > a -> sufficient
		// [a, b]
		// - x < a -> empty not accepted
		// - x = a -> empty accepted
		// - a < x < b/2 -> critical
		// - b/2 <= x < b -> sufficient
		// - x >= b -> full
		// [0, b]
		if (quantity < minQuantity)
			return Status.EmptyNotAccepted;
		if (quantity == minQuantity)
			return Status.EmptyAccepted;
			
		if (maxQuantity == null && quantity > minQuantity)
			return Status.Sufficient;
			
		if (maxQuantity != null)
		{
			var middlePoint = maxQuantity / 2;
			if (quantity < middlePoint)
				return Status.Critical;
			if (quantity < maxQuantity)
				return Status.Sufficient;
			return Status.Full;
		}
		
		return Status.EmptyAccepted;
	}
	
	public async Task BulkInsertAsync (IEnumerable<Guid> itemIds)
	{
		var models = itemIds.Select (
			id => new Models.Inventory
			{
				ItemId = id,
				UserId = user.Id,
				Status = GetStatusFromQuantity (0, 0)
			}
		)
		.ToList();
		
		await inventoryRepository.BulkInsertAsync (models);
	}
	
	public async Task<InventoryResponse?> UpdateAsync (Guid key, InventoryUpdateRequest model)
	{
		var oldInventory = await inventoryRepository.GetOneAsync (key, user.Id) ??
							throw new EntityNotFoundException (nameof (Models.Inventory), key);
		
		var realMinQuantity = model.MinQuantity ?? oldInventory.MinQuantity;
		var realMaxQuantity = model.MaxQuantity ?? oldInventory.MaxQuantity;
		
		if (model.MinQuantity != null && model.MinQuantity < 0)
			throw new ArgumentException ("Minimum quantity must be positive (>= 0)");
		if (model.MaxQuantity != null && model.MaxQuantity <= 0)
			throw new ArgumentException ("Max quantity must be positive (> 0)");
		if (model.MaxQuantity != null && model.MaxQuantity < realMinQuantity)
			throw new ArgumentException ("Max quantity must be greater than min quantity");

		var itemExists = await itemsService.ExistsAsync (key);
		if (!itemExists)
			throw new EntityNotFoundException (nameof (Item), key);
		
		var inventory = mapper.Map<InventoryUpdateRequest, Models.Inventory> (model);
		
		inventory.Status = GetStatusFromQuantity (0, minQuantity: realMinQuantity, maxQuantity: realMaxQuantity);
		
		var updated = await inventoryRepository.UpdateAsync (key, user.Id, inventory);
		var dto = mapper.Map<Models.Inventory, InventoryResponse> (updated!);
		
		return dto;
	}
	
	public async Task<bool> UpdateInventoryQuantitiesAsync (Dictionary<Guid, float> quantities)
	{
		var changedQuantities = quantities.Where(q => q.Value != 0).ToDictionary();
		
		var inventories = await inventoryRepository.GetAllFromListAsync (user.Id, changedQuantities.Keys);
		if (inventories.Count < changedQuantities.Count)
			throw new EntityNotFoundException (nameof (Models.Inventory), changedQuantities.Keys);
		var allItemsExist = await itemsService.AllExistAsync (changedQuantities.Keys);
		if (!allItemsExist)
			throw new EntityNotFoundException (nameof (Item), changedQuantities.Keys);
			
		List<Models.Inventory> updatedInventories = [];
		foreach (var newQuantity in changedQuantities)
		{
			var inventory = inventories[newQuantity.Key];
			inventory.QuantityInStock = newQuantity.Value;
			inventory.Status = GetStatusFromQuantity (newQuantity.Value, minQuantity: inventory.MinQuantity, maxQuantity: inventory.MaxQuantity);
			updatedInventories.Add (inventory);
		}
		await inventoryRepository.BulkUpdateAsync (user.Id, updatedInventories);
		
		return true;
	}
	
	public async Task<bool> BulkDeleteAsync (IEnumerable<Guid> keys)
	{
		try
		{
			return await inventoryRepository.BulkDeleteAsync (user.Id, keys);
		}
		catch (Exception)
		{
			return false;
		}
	}
}