namespace Storet.Core.Exceptions;

public sealed class MalformedRequestException : BadRequestException
{
	public MalformedRequestException (string message) : base (message)
	{}
}