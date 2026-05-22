using System.Net;

namespace Storet.Core.Exceptions;

public abstract class StoretException : Exception
{
	public HttpStatusCode StatusCode { get; }
	
	public StoretException (HttpStatusCode statusCode, string message) : base (message)
	{
		StatusCode = statusCode;
	}
}