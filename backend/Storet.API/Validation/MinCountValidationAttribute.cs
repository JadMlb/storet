using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Storet.API.Validation;

public class MinCountAttribute : ValidationAttribute
{
	private readonly int minCount;
	private readonly bool allowNull;
	
	public MinCountAttribute (int minCount, bool allowNull = false)
	{
		this.minCount = minCount;
		this.allowNull = allowNull;
		ErrorMessage = $"List must contain at least ${minCount} element{(minCount > 1 ? "s" : "")}";
	}
	
	public override bool IsValid (object? value)
	{
		if (value == null)
			return allowNull;
			
		if (value is ICollection collection)
			return collection.Count >= minCount;
			
		return false;
	}
}