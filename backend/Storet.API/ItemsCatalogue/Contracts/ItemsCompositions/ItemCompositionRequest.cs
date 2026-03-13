using System.ComponentModel.DataAnnotations;

namespace Storet.API.ItemsCatalogue.Contracts.ItemsCompositions;

public class ItemCompositionRequest
{
	public Guid? Id { get; set; }
	public string? Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	[Range (1, short.MaxValue)]
	public short Quantity { get; set; }
	[Required (AllowEmptyStrings = false)]
	public string Unit { get; set; } = string.Empty;
}