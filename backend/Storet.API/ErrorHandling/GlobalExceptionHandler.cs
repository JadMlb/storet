using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Storet.Core.Exceptions;

namespace Storet.API.ErrorHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
	private readonly IProblemDetailsService problemDetailsService;

	public GlobalExceptionHandler (IProblemDetailsService problemDetailsService)
	{
		this.problemDetailsService = problemDetailsService;
	}

	public async ValueTask<bool> TryHandleAsync (HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		int statusCode;
		string detail;
		
		if (exception is StoretException ex)
		{
			statusCode = (int) ex.StatusCode;
			detail = ex.Message;
		}
		else
		{
			statusCode = StatusCodes.Status500InternalServerError;
			detail = "An unexpected error occurred";
		}
		
		return await problemDetailsService.TryWriteAsync (new ProblemDetailsContext
			{
				HttpContext = httpContext,
				ProblemDetails = new ProblemDetails
				{
					Detail = detail,
					Status = statusCode
				}
			}
		);
	}
}