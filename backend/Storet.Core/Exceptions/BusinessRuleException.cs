namespace Storet.Core.Exceptions;

public sealed class BusinessRuleException : BadRequestException
{
	public BusinessRuleException (string message) : base (message)
	{}
}