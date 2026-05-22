using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Storet.API.ErrorHandling;

public class GlobalProblemDetailsFactory : ProblemDetailsFactory
{
	public override ProblemDetails CreateProblemDetails (HttpContext httpContext, int? statusCode = null, string? title = null, string? type = null, string? detail = null, string? instance = null)
	{
		statusCode ??= 500;

		var problemDetails = new ProblemDetails
		{
			Status = statusCode,
			Title = title,
			Type = type,
			Detail = detail,
			Instance = instance ?? httpContext.Request.Path
		};

		problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

		return problemDetails;
	}
	
	public override ValidationProblemDetails CreateValidationProblemDetails (HttpContext httpContext, ModelStateDictionary modelStateDictionary, int? statusCode = null, string? title = null, string? type = null, string? detail = null, string? instance = null)
	{
		statusCode ??= 400;
		title ??= "Validation failed";

		var validationProblemDetails = new ValidationProblemDetails (modelStateDictionary)
		{
			Status = statusCode,
			Title = title,
			Type = type,
			Detail = detail,
			Instance = instance ?? httpContext.Request.Path
		};

		validationProblemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

		return validationProblemDetails;
	}
}