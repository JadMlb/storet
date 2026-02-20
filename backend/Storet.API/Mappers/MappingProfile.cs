using AutoMapper;
using Storet.API.Contracts.Categories;
using Storet.API.Models;

namespace Storet.API.Mappers;

public class MappingProfile : Profile
{
	public MappingProfile ()
	{
		#region Categories
		CreateMap<Category, CategoryResponse>()
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
	}
}