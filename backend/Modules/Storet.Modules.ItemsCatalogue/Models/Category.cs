using System.ComponentModel.DataAnnotations;

namespace Storet.Modules.ItemsCatalogue.Models;

public class Category
{
	public int Id { get; set; }
	[Required (AllowEmptyStrings = false)]
	public string Label { get; set; } = string.Empty;
	public int? ParentCategoryId { get; set; }
	public Category? ParentCategory { get; set; }
	public ICollection<Category> SubCategories { get; set; } = [];
	public Guid UserId { get; set; }
}