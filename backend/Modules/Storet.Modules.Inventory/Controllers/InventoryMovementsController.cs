using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storet.Core.Exceptions;
using Storet.Core.Utils;
using Storet.Modules.Inventory.Contracts.InventoryMovement;
using Storet.Modules.Inventory.Services.InventoryMovements;

namespace Storet.Modules.Inventory.Controllers;

[ApiController]
[Route ("api/transactions")]
[Authorize]
public class InventoryMovementsController : ControllerBase
{
	private readonly IInventoryMovementsService service;
	
	public InventoryMovementsController (IInventoryMovementsService service)
	{
		this.service = service;
	}
	
	[HttpGet]
	public async Task<ActionResult<PaginatedResponse<InventoryMovementResponse, DateTimeOffset?>>> GetAll ([FromQuery] Query<DateTimeOffset?> query)
	{
		return Ok (await service.GetAllAsync (query));
	}
	
	[HttpGet ("{id:guid}")]
	public async Task<ActionResult<InventoryMovementDetailsResponse>> GetOne ([FromRoute] Guid id)
	{
		var transaction = await service.GetOneAsync (id);
		if (transaction == null)
			return NotFound();
			
		return Ok (transaction);
	}
	
	[HttpPost]
	public async Task<ActionResult<IEnumerable<InventoryMovementDetailsResponse>>> Log ([FromBody] InventoryModificationRequest request)
	{
		if (!ModelState.IsValid)
			return BadRequest (ModelState);
			
		var inserted = await service.InsertAsync (request);
		if (inserted == null)
			return BadRequest();
		return Ok (inserted);
	}
}