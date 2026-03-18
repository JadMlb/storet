namespace Storet.Modules.ItemsCatalogue.Models;

public class ItemCategory
{
	public int CategoryId { get; set; }
	public Category Category { get; set; } = null!;
	public Guid ItemId { get; set; }
	public Item Item { get; set; } = null!;
}