using System.ComponentModel.DataAnnotations;

namespace Storet.Core.Validation;

public class PositiveValueAttribute : ValidationAttribute
{
	private readonly bool include;
	
	public PositiveValueAttribute (bool include = false)
	{
		this.include = include;
		ErrorMessage = $"Value must be positive (>{(include ? "=" : "")} 0)";
	}

	public override bool IsValid (object? value)
	{
		if (value == null)
			return true;

		try
		{
			var doubleValue = Convert.ToDouble (value);
			return include ? doubleValue >= 0 : doubleValue > 0;
		}
		catch (Exception)
		{
			return false;
		}
	}
}