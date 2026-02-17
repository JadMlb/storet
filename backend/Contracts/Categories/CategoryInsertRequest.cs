using System.ComponentModel.DataAnnotations;

namespace Storet.Backend.Contracts.Categories;

public class CategoryInsertRequest
{
	[Required (AllowEmptyStrings = false)]
	public string Label { get; set; } = string.Empty;
	public int? ParentCategoryId { get; set; }
}