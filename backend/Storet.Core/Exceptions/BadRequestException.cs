using System.Net;

namespace Storet.Core.Exceptions;

public abstract class BadRequestException : StoretException
{
	public BadRequestException (string message) : base (HttpStatusCode.BadRequest, message)
	{}
}