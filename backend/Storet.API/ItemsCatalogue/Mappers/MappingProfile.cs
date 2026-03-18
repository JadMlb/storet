using AutoMapper;
using Storet.API.ItemsCatalogue.Contracts.Categories;
using Storet.API.ItemsCatalogue.Contracts.Items;
using Storet.API.ItemsCatalogue.Contracts.ItemsCompositions;
using Storet.API.ItemsCatalogue.Models;

namespace Storet.API.ItemsCatalogue.Mappers;

public class MappingProfile : Profile
{
	public MappingProfile ()
	{
		#region Categories
		CreateMap<Category, CategoryResponseWithSubCategories>()
			.ForMember (
				dest => dest.SubCategories,
				opt => opt.MapFrom (src => src.SubCategories)
			)
			.ForSourceMember (c => c.ParentCategory, opt => opt.DoNotValidate());
		
		CreateMap<Category, CategoryResponseWithParent>()
			.ForMember (
				dest => dest.SubCategories,
				opt => opt.MapFrom (src => src.SubCategories)
			);
		
		CreateMap<Category, CategoryResponse>()
			.ForSourceMember (c => c.SubCategories, opt => opt.DoNotValidate())
			.ForSourceMember (c => c.ParentCategory, opt => opt.DoNotValidate())
			.ForSourceMember (c => c.ParentCategoryId, opt => opt.DoNotValidate());

		CreateMap<CategoryInsertRequest, Category>()
			.ForMember (dest => dest.Id, opt => opt.Ignore())
			.ForMember (dest => dest.SubCategories, opt => opt.Ignore())
			.ForMember (dest => dest.ParentCategory, opt => opt.Ignore());
		
		CreateMap<CategoryUpdateRequest, Category>()
			.ForMember (dest => dest.Id, opt => opt.Ignore())
			.ForMember (dest => dest.SubCategories, opt => opt.Ignore())
			.ForMember (dest => dest.ParentCategory, opt => opt.Ignore())
			.ForAllMembers (opts => opts.Condition ((src, dest, srcMember) => srcMember != null));
		
		CreateMap<CategoryHierarchy, Category>()
			.ForSourceMember (h => h.Level, opt => opt.DoNotValidate())
			.ForMember (c => c.SubCategories, opt => opt.Ignore());
		#endregion
		
		#region Items
		CreateMap<Item, ItemResponse>()
			.ForSourceMember (i => i.ItemCategories, opt => opt.DoNotValidate())
			.ForSourceMember (i => i.Components, opt => opt.DoNotValidate())
			.ForSourceMember (i => i.Quantity, opt => opt.DoNotValidate())
			.ForSourceMember (i => i.Unit, opt => opt.DoNotValidate())
			.ForSourceMember (i => i.IsComponent, opt => opt.DoNotValidate());
		CreateMap<Item, ItemResponseDetails>()
			.ForSourceMember (i => i.IsComponent, opt => opt.DoNotValidate())
			.ForMember (i => i.Categories, opt => opt.MapFrom (i => i.ItemCategories.Select (i => i.Category)))
			.ForMember (i => i.Components, opt => opt.MapFrom (i => i.Components));
			
		CreateMap<ItemComposition, ItemCompositionResponse>()
			.ForMember (i => i.Id, opt => opt.MapFrom (i => i.ComponentItemId))
			.ForMember (i => i.Name, opt => opt.MapFrom (i => i.ComponentItem.Name))
			.ForMember (i => i.Description, opt => opt.MapFrom (i => i.ComponentItem.Description));
		
		CreateMap<ItemInsertRequest, Item>()
			.ForMember (i => i.Id, opt => opt.Ignore())
			.ForMember (i => i.ItemCategories, opt => opt.Ignore())
			.ForMember (i => i.Components, opt => opt.Ignore());
		
		CreateMap<ItemUpdateRequest, Item>()
			.ForMember (i => i.Id, opt => opt.Ignore())
			.ForMember (i => i.ItemCategories, opt => opt.Ignore())
			.ForMember (i => i.Components, opt => opt.Ignore())
			.ForAllMembers (opts => opts.Condition ((src, dest, srcMember) => srcMember != null));
		#endregion Items
	}
}