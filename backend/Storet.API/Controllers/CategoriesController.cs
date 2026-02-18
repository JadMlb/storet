using Microsoft.AspNetCore.Mvc;
using Storet.API.Contracts.Categories;
using Storet.API.Services.Categories;

namespace Storet.API.Controllers;

[ApiController]
[Route ("api/categories")]
public class CategoriesController : ControllerBase
{
	private readonly ICategoriesService categories;

	public CategoriesController (ICategoriesService categories)
	{
		this.categories = categories;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAll ()
	{
		return Ok (await categories.GetAllAsync());
	}

	[HttpGet ("{id:int}")]
	public async Task<ActionResult<CategoryResponse>> GetOne (int id)
	{
		var category = await categories.GetOneAsync (id);
		if (category == null)
			return NotFound();
		return Ok (category);
	}

	[HttpPost]
	public async Task<ActionResult<CategoryResponse>> Create ([FromBody] CategoryInsertRequest category)
	{
		if (!ModelState.IsValid)
			return BadRequest (ModelState);

		var inserted = await categories.InsertAsync (category);
		if (inserted == null)
			return Problem ("Something went wrong while inserting the category");
		return CreatedAtAction (nameof (GetOne), new {inserted.Id}, inserted);
	}
	
	[HttpPut ("{id:int}")]
	public async Task<ActionResult<CategoryResponse>> Update ([FromRoute] int id,[FromBody] CategoryUpdateRequest category)
	{
		if (!ModelState.IsValid)
			return BadRequest (ModelState);

		var updated = await categories.UpdateAsync (id, category);
		if (updated == null)
			return NotFound();
		return Ok (updated);
	}
	
	[HttpDelete ("{id:int}")]
	public async Task<ActionResult> Delete ([FromRoute] int id)
	{
		try
		{
			var deleted = await categories.DeleteAsync (id);
			if (!deleted)
				return NotFound();
			return NoContent();
		}
		catch (Exception)
		{
			return BadRequest();
		}
	}
}