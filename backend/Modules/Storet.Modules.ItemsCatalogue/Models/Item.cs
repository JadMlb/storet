using System.ComponentModel.DataAnnotations;

namespace Storet.Modules.ItemsCatalogue.Models;

public class Item
{
	public Guid Id { get; set; }
	[Required (AllowEmptyStrings = false)]
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	[Range (float.Epsilon, float.MaxValue)]
	public float Quantity { get; set; }
	public Unit Unit { get; set; }
	public bool IsComponent { get; set; } = false;
	public ICollection<ItemCategory> ItemCategories { get; set; } = null!;
	public ICollection<ItemComposition> Components { get; set; } = null!;
}