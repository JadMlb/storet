using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Storet.Core.Mappers;
using Storet.Core.Validation;
using Storet.Modules.ItemsCatalogue.Contracts.ItemsCompositions;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Contracts.Items;

public class ItemUpdateRequest
{
	[MinLength (1)]
	public string? Name { get; set; }
	public string? Description { get; set; }
	public List<int>? Categories { get; set; }
}