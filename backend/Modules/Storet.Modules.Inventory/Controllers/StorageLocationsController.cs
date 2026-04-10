using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storet.Core.Exceptions;
using Storet.Modules.Inventory.Contracts.StorageLocation;
using Storet.Modules.Inventory.Services.StorageLocations;

namespace Storet.Modules.Inventory.Controllers;

[ApiController]
[Authorize]
[Route ("api/storage")]
public class StorageLocationsController : ControllerBase
{
	private readonly IStorageLocationsService storageLocationsService;
	
	public StorageLocationsController (IStorageLocationsService storageLocationsService)
	{
		this.storageLocationsService = storageLocationsService;
	}
	
	[HttpGet]
	public async Task<ActionResult<IEnumerable<StorageLocationResponse>>> GetAll ()
	{
		return Ok (await storageLocationsService.GetAllAsync());
	}
	
	[HttpGet ("{id:guid}")]
	public async Task<ActionResult<StorageLocationDetailsResponse>> GetOne (Guid id)
	{
		var location = await storageLocationsService.GetOneAsync (id);
		if (location == null)
			return NotFound();
		return Ok (location);
	}
	
	[HttpPost]
	public async Task<ActionResult<StorageLocationDetailsResponse>> Create ([FromBody] StorageLocationInsertRequest location)
	{
		if (!ModelState.IsValid)
			return BadRequest (ModelState);

		try
		{
			var inserted = await storageLocationsService.InsertAsync (location);
			if (inserted == null)
				return Problem ("Something went wrong while inserting the storage location");
			return CreatedAtAction (nameof (GetOne), new {inserted.Id}, inserted);
		}
		catch (DuplicateKeyException e)
		{
			return Conflict (e.Message);
		}
		catch (Exception e)
		{
			return BadRequest (e.Message);
		}
	}
	
	[HttpPut ("{id:guid}")]
	public async Task<ActionResult<StorageLocationDetailsResponse>> Update ([FromRoute] Guid id,[FromBody] StorageLocationUpdateRequest location)
	{
		if (!ModelState.IsValid)
			return BadRequest (ModelState);

		try
		{
			var updated = await storageLocationsService.UpdateAsync (id, location);
			if (updated == null)
				return NotFound();
			return Ok (updated);
		}
		catch (DuplicateKeyException e)
		{
			return Conflict (e.Message);
		}
		catch (Exception e)
		{
			return BadRequest (e.Message);
		}
	}
	
	[HttpDelete ("{id:guid}")]
	public async Task<ActionResult> Delete ([FromRoute] Guid id)
	{
		try
		{
			var deleted = await storageLocationsService.DeleteAsync (id);
			if (!deleted)
				return NotFound();
			return NoContent();
		}
		catch (EntityDependencyException e)
		{
			return Conflict (e.Message);
		}
	}
}