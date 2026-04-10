using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Storet.Tests.Common.Controller;

public abstract class ControllerTestsBase<TController, TService> where TService : class where TController : ControllerBase
{
	protected readonly Mock<TService> mockService;
	protected readonly TController controller;
	
	public ControllerTestsBase ()
	{
		mockService = new Mock<TService>();
		controller = InitControllerInstance();
	}
	
	protected abstract TController InitControllerInstance ();
	
	protected void ValidateModel<T> (T model)
	{
		var validationContext = new ValidationContext (model!);
		var validationResults = new List<ValidationResult>();
		
		Validator.TryValidateObject (model!, validationContext, validationResults, true);
		
		foreach (var result in validationResults)
			foreach (var memberName in result.MemberNames)
				controller.ModelState.AddModelError (memberName, result.ErrorMessage ?? "");
	}
}