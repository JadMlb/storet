using AutoMapper;
using Storet.Core.Mappers;
using Storet.Modules.Inventory.Contracts.Inventory;
using Storet.Modules.Inventory.Contracts.StorageLocation;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Mappers;

public class MappingProfile : Profile
{
	public MappingProfile ()
	{
		CreateMap<InventoryInsertRequest, Models.Inventory>()
			.ForMember (dest => dest.QuantityInStock, opts => opts.Ignore())
			.ForMember (dest => dest.Status, opts => opts.Ignore())
			.ForMember (dest => dest.UserId, opts => opts.MapFrom<CurrentUserResolver<InventoryInsertRequest, Models.Inventory>>());
		CreateMap<InventoryUpdateRequest, Models.Inventory>()
			.ForMember (dest => dest.ItemId, opts => opts.Ignore())
			.ForMember (dest => dest.QuantityInStock, opts => opts.Ignore())
			.ForMember (dest => dest.Status, opts => opts.Ignore())
			.ForMember (dest => dest.UserId, opts => opts.MapFrom<CurrentUserResolver<InventoryUpdateRequest, Models.Inventory>>())
			.ForAllMembers (opts => opts.Condition ((src, dest, srcMember) => srcMember != null));
		CreateMap<Models.Inventory, InventoryResponse>()
			.ForSourceMember (src => src.UserId, opts => opts.DoNotValidate())
			.ForMember (dest => dest.Item, opts => opts.Ignore());
			
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
	}
}