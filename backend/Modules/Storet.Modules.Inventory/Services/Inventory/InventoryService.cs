using AutoMapper;
using Storet.Core.Authorization;
using Storet.Core.Exceptions;
using Storet.Modules.Inventory.Contracts.Inventory;
using Storet.Modules.Inventory.Models;
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
	
	public async Task<IEnumerable<InventoryResponse>> GetAllAsync ()
	{
		var inventories = await inventoryRepository.GetAllAsync (user.Id);
		
		if (!inventories.Any())
			return [];
			
		var itemIds = inventories.Select (i => i.ItemId);
		var items = await itemsService.GetAllFromListAsync (itemIds);
		
		List<InventoryResponse> result = [];
		foreach (var inventory in inventories)
		{
			var dto = mapper.Map<Models.Inventory, InventoryResponse> (inventory);
			dto.Item = items?.GetValueOrDefault (inventory.ItemId)!;
			result.Add (dto);
		}
		
		return result;
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
	
	public async Task<IEnumerable<InventoryResponse>> BulkInsertAsync (IEnumerable<Guid> itemIds)
	{
		var items = await itemsService.GetAllFromListAsync (itemIds);
		var notFoundItems = itemIds.Except(items?.Select (i => i.Key) ?? []).ToList();
		if (items == null || notFoundItems.Count > 0)
			throw new EntityNotFoundException (nameof (Item), itemIds);
		
		var models = itemIds.Select (
			id => new Models.Inventory
			{
				ItemId = id,
				UserId = user.Id,
				Status = GetStatusFromQuantity (0, 0)
			}
		)
		.ToList();
		
		var inserted = await inventoryRepository.BulkInsertAsync (models);
		return models.Select (
			i => new InventoryResponse
			{
				Item = items[i.ItemId],
				Status = i.Status
			}
		)
		.ToList();
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

		var item = await itemsService.CheckIfExistsAndGetMetadataAsync (key) ??
						throw new EntityNotFoundException (nameof (Item), key);
		
		var inventory = mapper.Map<InventoryUpdateRequest, Models.Inventory> (model);
		
		inventory.Status = GetStatusFromQuantity (0, minQuantity: realMinQuantity, maxQuantity: realMaxQuantity);
		
		var updated = await inventoryRepository.UpdateAsync (key, user.Id, inventory);
		var dto = mapper.Map<Models.Inventory, InventoryResponse> (updated!);
		dto.Item = item;
		
		return dto;
	}
	
	public async Task<bool> UpdateInventoryQuantitiesAsync (Dictionary<Guid, float> quantities)
	{
		var changedQuantities = quantities.Where(q => q.Value != 0).ToDictionary();
		
		var inventories = await inventoryRepository.GetAllFromListAsync (user.Id, changedQuantities.Keys);
		if (inventories.Count < changedQuantities.Count)
			throw new EntityNotFoundException (nameof (Models.Inventory), changedQuantities.Keys);
		var items = await itemsService.GetAllFromListAsync (changedQuantities.Keys);
		if ((items?.Count ?? 0) < changedQuantities.Count)
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