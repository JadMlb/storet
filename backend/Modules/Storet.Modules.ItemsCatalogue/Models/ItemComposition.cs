using System.ComponentModel.DataAnnotations;

namespace Storet.Modules.ItemsCatalogue.Models;

public class ItemComposition
{
	public Guid ParentItemId { get; set; }
	public Guid ComponentItemId { get; set; }
	public Item ComponentItem { get; set; } = null!;
	[Range ((short) 1, short.MaxValue)]
	public short Quantity { get; set; }
	public Guid UserId { get; set; }
}