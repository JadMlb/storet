using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storet.Core.Exceptions;
using Storet.Core.Utils;
using Storet.Modules.Inventory.Contracts.Inventory;
using Storet.Modules.Inventory.Queries;
using Storet.Modules.Inventory.Services.Inventory;

namespace Storet.Modules.Inventory.Controllers;

[ApiController]
[Route ("api/inventory")]
[Authorize]
public class InventoryController : ControllerBase
{
	private readonly IInventoryService service;
	
	public InventoryController (IInventoryService service)
	{
		this.service = service;
	}
	
	[HttpGet]
	public async Task<ActionResult<IEnumerable<InventoryResponse>>> GetAll ([FromQuery] InventoryFilterQuery query)
	{
		return Ok (await service.GetAllAsync (query));
	}

	[HttpGet ("{id:guid}")]
	public async Task<ActionResult<InventoryResponse?>> GetOne (Guid id)
	{
		var inventory = await service.GetOneAsync (id);
		if (inventory == null)
			return NotFound();
		return Ok (inventory);
	}
	
	[HttpPut ("{id:guid}")]
	public async Task<ActionResult<InventoryResponse>> Update ([FromRoute] Guid id, [FromBody] InventoryUpdateRequest updateRequest)
	{
		if (!ModelState.IsValid)
			return BadRequest (ModelState);
			
		var updated = await service.UpdateAsync (id, updateRequest);
		if (updated == null)
			return NotFound();
		return Ok (updated);
	}
}