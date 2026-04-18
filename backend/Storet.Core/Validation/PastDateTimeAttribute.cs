using System.ComponentModel.DataAnnotations;

namespace Storet.Core.Validation;

public class PastDateTimeAttribute : ValidationAttribute
{
	public PastDateTimeAttribute ()
	{
		ErrorMessage = "Future date or time";
	}
	
	public override bool IsValid (object? obj)
	{
		if (obj is not DateTimeOffset timestampValue)
			return false;
			
		return timestampValue <= DateTimeOffset.Now;
	}
}