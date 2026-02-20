namespace Storet.API.Models;

public class CategoryHierarchy
{
	public int Id { get; set; }
	public string Label { get; set; } = string.Empty;
	public int? ParentCategoryId { get; set; }
	public int Level { get; set; }
}