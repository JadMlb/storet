using AutoMapper;
using Storet.Core.Mappers;
using Storet.Modules.Inventory.Contracts.Inventory;
using Storet.Modules.Inventory.Contracts.InventoryMovement;
using Storet.Modules.Inventory.Contracts.StorageLocation;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Mappers;

public class MappingProfile : Profile
{
	public MappingProfile ()
	{
		CreateMap<InventoryUpdateRequest, Models.Inventory>()
			.ForMember (dest => dest.ItemId, opts => opts.Ignore())
			.ForMember (dest => dest.QuantityInStock, opts => opts.Ignore())
			.ForMember (dest => dest.Status, opts => opts.Ignore())
			.ForMember (dest => dest.UserId, opts => opts.MapFrom<CurrentUserResolver<InventoryUpdateRequest, Models.Inventory>>())
			.ForAllMembers (opts => opts.Condition ((src, dest, srcMember) => srcMember != null));
		CreateMap<Models.Inventory, InventoryResponse>()
			.ForSourceMember (src => src.UserId, opts => opts.DoNotValidate());
			
		CreateMap<StorageLocation, StorageLocationResponse>()
			.ForSourceMember (l => l.UserId, opts => opts.DoNotValidate())
			.ForSourceMember (l => l.Description, opts => opts.DoNotValidate());
		CreateMap<StorageLocation, StorageLocationDetailsResponse>()
			.ForSourceMember (l => l.UserId, opts => opts.DoNotValidate());
		CreateMap<StorageLocationInsertRequest, StorageLocation>()
			.ForMember (dest => dest.Id, opts => opts.Ignore())
			.ForMember (dest => dest.UserId, opts => opts.MapFrom<CurrentUserResolver<StorageLocationInsertRequest, StorageLocation>>());
		CreateMap<StorageLocationUpdateRequest, StorageLocation>()
			.ForMember (dest => dest.Id, opts => opts.Ignore())
			.ForMember (dest => dest.UserId, opts => opts.MapFrom<CurrentUserResolver<StorageLocationUpdateRequest, StorageLocation>>())
			.ForAllMembers (opts => opts.Condition ((src, dest, srcMember) => srcMember != null));
			
		CreateMap<InventoryMovement, InventoryMovementResponse>()
			.ForMember (dest => dest.Location, opts => opts.MapFrom ((src, dest, srcMember) => src.StorageLocation))
			.ForSourceMember (src => src.StorageLocationId, opts => opts.DoNotValidate())
			.ForSourceMember (src => src.StorageLocation, opts => opts.DoNotValidate())
			.ForSourceMember (src => src.UserId, opts => opts.DoNotValidate())
			.ForSourceMember (src => src.Items, opts => opts.DoNotValidate());
		CreateMap<InventoryMovement, InventoryMovementDetailsResponse>()
			.ForMember (dest => dest.Items, opts => opts.Ignore())
			.ForMember (dest => dest.Location, opts => opts.MapFrom ((src, dest, srcMember) => src.StorageLocation))
			.ForSourceMember (src => src.StorageLocationId, opts => opts.DoNotValidate())
			.ForSourceMember (src => src.StorageLocation, opts => opts.DoNotValidate())
			.ForSourceMember (src => src.UserId, opts => opts.DoNotValidate())
			.ForSourceMember (src => src.Items, opts => opts.DoNotValidate());
		CreateMap<InventoryModificationRequest, InventoryMovement>()
			.ForSourceMember (src => src.Items, opts => opts.DoNotValidate())
			.ForSourceMember (src => src.DestinationLocationId, opts => opts.DoNotValidate())
			.ForSourceMember (src => src.SourceLocationId, opts => opts.DoNotValidate())
			.ForMember (dest => dest.Direction, opts => opts.Ignore())
			.ForMember (dest => dest.StorageLocationId, opts => opts.Ignore())
			.ForMember (dest => dest.Items, opts => opts.Ignore())
			.ForMember (dest => dest.Source, opts => opts.MapFrom<InventoryMovementSourceValueResolver>())
			.ForMember (dest => dest.NumberOfItems, opts => opts.MapFrom<InventoryMovementNumberOfItemsValueResolver>())
			.ForMember (dest => dest.UserId, opts => opts.MapFrom<CurrentUserResolver<InventoryModificationRequest, InventoryMovement>>());
	}
}