using System.ComponentModel.DataAnnotations;

namespace Storet.Backend.Contracts.Categories;

public class CategoryUpdateRequest
{
	public string? Label { get; set; }
	public int? ParentCategoryId { get; set; }
}