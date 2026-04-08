using AutoMapper;
using Storet.Core.Mappers;
using Storet.Modules.Inventory.Contracts.Inventory;

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
	}
}