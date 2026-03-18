using Microsoft.AspNetCore.Mvc;
using Storet.Modules.ItemsCatalogue.Contracts.Items;
using Storet.Core.Exceptions;
using Storet.Core.Utils;
using Storet.Modules.ItemsCatalogue.Services.Items;

namespace Storet.Modules.ItemsCatalogue.Controllers;

[ApiController]
[Route ("api/items")]
public class ItemsController : ControllerBase
{
	private readonly IItemsService service;
	
	public ItemsController (IItemsService service)
	{
		this.service = service;
	}
	
	[HttpGet]
	public async Task<ActionResult<IEnumerable<ItemResponse>>> GetAll ([FromQuery] Query<string> query)
	{
		return Ok (await service.GetAllAsync (query));
	}
	
	[HttpGet ("components")]
	public async Task<ActionResult<IEnumerable<ItemResponse>>> GetAllComponents ()
	{
		return Ok (await service.GetAllComponentsAsync());
	}
	
	[HttpGet ("{id:guid}")]
	public async Task<ActionResult<ItemResponseDetails>> GetOne ([FromRoute] Guid id)
	{
		var item = await service.GetOneAsync (id);
		if (item == null)
			return NotFound();
		return Ok (item);
	}
	
	[HttpPost]
	public async Task<ActionResult<ItemResponseDetails>> Create ([FromBody] ItemInsertRequest insertRequest)
	{
		if (!ModelState.IsValid)
			return BadRequest (ModelState);
			
		try
		{
			var inserted = await service.InsertAsync (insertRequest);
			return CreatedAtAction (nameof (GetOne), new {inserted!.Id}, inserted);
		}
		catch (EntityNotFoundException e)
		{
			return NotFound (e.Message);
		}
		catch (Exception e)
		{
			return BadRequest (e.Message);
		}
	}
	
	[HttpPut ("{id:guid}")]
	public async Task<ActionResult<ItemResponseDetails>> Update ([FromRoute] Guid id, [FromBody] ItemUpdateRequest updatedValues)
	{
		if (!ModelState.IsValid)
			return BadRequest (ModelState);
		
		try
		{
			var updated = await service.UpdateAsync (id, updatedValues);
			if (updated == null)
				return NotFound();
			return Ok (updated);
		}
		catch (EntityNotFoundException e)
		{
			return NotFound (e.Message);
		}
	}
	
	[HttpDelete ("{id:guid}")]
	public async Task<ActionResult> Delete ([FromRoute] Guid id)
	{
		var deleted = await service.DeleteAsync (id);
		if (!deleted)
			return NotFound();
		return NoContent();
	}
}