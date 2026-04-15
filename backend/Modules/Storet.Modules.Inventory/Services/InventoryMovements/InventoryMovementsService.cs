using AutoMapper;
using Storet.Core.Authorization;
using Storet.Core.Exceptions;
using Storet.Core.Utils;
using Storet.Modules.Inventory.Contracts.InventoryMovement;
using Storet.Modules.Inventory.Models;
using Storet.Modules.Inventory.Repositories.InventoryMovementItems;
using Storet.Modules.Inventory.Repositories.InventoryMovements;
using Storet.Modules.Inventory.Repositories.StorageLocations;
using Storet.Modules.Inventory.Services.Inventory;
using Storet.Modules.ItemsCatalogue.Contracts.Items;
using Storet.Modules.ItemsCatalogue.Models;
using Storet.Modules.ItemsCatalogue.Services.Items;

namespace Storet.Modules.Inventory.Services.InventoryMovements;

public class InventoryMovementsService : IInventoryMovementsService
{
	private readonly IInventoryMovementsRepository inventoryMovementsRepository;
	private readonly IInventoryMovementItemsRepository inventoryMovementItemsRepository;
	private readonly IStorageLocationsRepository storageLocationsRepository;
	private readonly IInventoryService inventoryService;
	private readonly IItemsService itemsService;
	private readonly ICurrentUser user;
	private readonly IMapper mapper;
	
	public InventoryMovementsService (IInventoryMovementsRepository inventoryMovementsRepository, IInventoryMovementItemsRepository inventoryMovementItemsRepository, IStorageLocationsRepository storageLocationsRepository, IInventoryService inventoryService, IItemsService itemsService, ICurrentUser user, IMapper mapper)
	{
		this.inventoryMovementsRepository = inventoryMovementsRepository;
		this.inventoryMovementItemsRepository = inventoryMovementItemsRepository;
		this.inventoryService = inventoryService;
		this.storageLocationsRepository = storageLocationsRepository;
		this.itemsService = itemsService;
		this.user = user;
		this.mapper = mapper;
	}

	public async Task<PaginatedResponse<InventoryMovementResponse, DateTimeOffset?>> GetAllAsync (Query<DateTimeOffset?> query)
	{
		if (query.PageSize < 1)
			throw new ArgumentException ("Invalid page size for query");
			
		var data = await inventoryMovementsRepository.GetAllAsync (query, user.Id);
		var previousKey = await inventoryMovementsRepository.GetPreviousKeyAsync (query, user.Id);
		
		var response = new PaginatedResponse<InventoryMovementResponse, DateTimeOffset?>();
		if (data.Count() > query.PageSize)
		{
			response.Next = data.Last().ExecutedAt;
			response.Data = data.Take(query.PageSize).Select(mapper.Map<InventoryMovement, InventoryMovementResponse>).ToList();
		}
		else
			response.Data = data.Select(mapper.Map<InventoryMovement, InventoryMovementResponse>).ToList();
			
		response.Previous = previousKey;
		
		return response;
	}

	public async Task<InventoryMovementDetailsResponse?> GetOneAsync (Guid key)
	{
		var entry = await inventoryMovementsRepository.GetOneAsync (key, user.Id);
		if (entry == null)
			return null;
		
		var items = await itemsService.GetAllFromListWithUnitAsync (entry.Items.Select (i => i.ItemId));
		var dto = mapper.Map<InventoryMovement, InventoryMovementDetailsResponse> (entry);
		foreach (var item in entry.Items)
			dto.Items.Add (
				new ()
				{
					Item = items[item.ItemId],
					Quantity = item.Quantity
				}
			);
		return dto;
	}
	
	private async Task<InventoryMovementDetailsResponse> LogMovement (InventoryModificationRequest model, MovementDirection direction, Guid storageLocationId, Dictionary<Guid, ItemResponseWithUnit> items)
	{
		var movementEntity = mapper.Map<InventoryModificationRequest, InventoryMovement> (model);
		movementEntity.Direction = direction;
		movementEntity.StorageLocationId = storageLocationId;
		var movement = await inventoryMovementsRepository.InsertAsync (movementEntity) ??
						throw new InvalidOperationException ("An error occured when trying to log the transaction");
		
		List<InventoryMovementItem> itemsInMovementToBeInserted = [];
		short ordinal = 1;
		List<InventoryMovementItemResponse> itemsInResponse = [];
		foreach (var itemQuantity in model.Items)
		{
			var item = new InventoryMovementItem
			{
				ItemId = itemQuantity.Key,
				MovementId = movement.Id,
				Ordinal = ordinal,
				Quantity = itemQuantity.Value,
				UserId = user.Id,
			};
			itemsInMovementToBeInserted.Add (item);
			
			var itemResponse = new InventoryMovementItemResponse
			{
				Item = items[itemQuantity.Key],
				Quantity = itemQuantity.Value
			};
			itemsInResponse.Add (itemResponse);
			
			ordinal++;
		}
		
		await inventoryMovementItemsRepository.BulkInsertAsync (itemsInMovementToBeInserted);
		
		var result = mapper.Map<InventoryMovement, InventoryMovementDetailsResponse> (movement);
		result.Items = itemsInResponse;
		
		return result;
	}

	public async Task<IEnumerable<InventoryMovementDetailsResponse>?> InsertAsync (InventoryModificationRequest model)
	{
		if (model.ExecutedAt > DateTimeOffset.Now)
			throw new ArgumentException ("This transaction is set to a future date or time");
			
		if (model.DestinationLocationId == null && model.SourceLocationId == null)
			throw new ArgumentException ("Neither source nor destination were specified");
		
		if (model.SourceLocationId.HasValue)
		{
			var destinationLocationExists = await storageLocationsRepository.ExistsAsync (model.SourceLocationId.Value, user.Id);
			if (!destinationLocationExists)
				throw new EntityNotFoundException (nameof (StorageLocation), model.SourceLocationId.Value);
		}
		
		if (model.DestinationLocationId.HasValue)
		{
			var destinationLocationExists = await storageLocationsRepository.ExistsAsync (model.DestinationLocationId.Value, user.Id);
			if (!destinationLocationExists)
				throw new EntityNotFoundException (nameof (StorageLocation), model.DestinationLocationId.Value);
		}
		
		var items = await itemsService.GetAllFromListWithUnitAsync (model.Items.Keys);
		if (items == null || items.Count != model.Items.Keys.Count)
			throw new EntityNotFoundException (nameof (Item), model.Items.Keys);
		
		List<InventoryMovementDetailsResponse> result = [];
		if (model.SourceLocationId.HasValue)
		{
			var sourceMovement = await LogMovement (model, MovementDirection.Out, model.SourceLocationId.Value, items);
			result.Add (sourceMovement);
		}
		
		if (model.DestinationLocationId.HasValue)
		{
			var destinationMovement = await LogMovement (model, MovementDirection.In, model.DestinationLocationId.Value, items);
			result.Add (destinationMovement);
		}
		
		// only update when items enter or leave, not on transfer
		if (result.Count == 1)
		{
			float sign = result.First().Direction == MovementDirection.In ? 1 : -1;
			var itemQuantitiesDiff = model.Items.ToDictionary (i => i.Key, i => i.Value * sign);
			await inventoryService.UpdateInventoryQuantitiesAsync (itemQuantitiesDiff);
		}
		
		return result;
	}
}