using System.ComponentModel.DataAnnotations;

namespace Storet.API.Core.Validation;

public class PositiveValueAttribute : ValidationAttribute
{
	public PositiveValueAttribute ()
	{
		ErrorMessage = $"Value must be positive (> 0)";
	}

	public override bool IsValid (object? value)
	{
		if (value == null)
			return true;

		try
		{
			var doubleValue = Convert.ToDouble (value);
			return doubleValue > 0;
		}
		catch (Exception)
		{
			return false;
		}
	}
}