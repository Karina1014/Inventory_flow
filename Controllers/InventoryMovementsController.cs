using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Flow.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InventoryMovementsController : ControllerBase
{
    private readonly IInventoryMovementRepository _movementRepo;

    public InventoryMovementsController(IInventoryMovementRepository movementRepo)
    {
        _movementRepo = movementRepo;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ICollection<InventoryMovementDto>>> GetMovements()
    {
        var list = await _movementRepo.GetMovementsAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}", Name = "GetInventoryMovement")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryMovementDto>> GetMovement(int id)
    {
        var item = await _movementRepo.GetMovementByIdAsync(id);
        if (item == null) return NotFound($"Movimiento con Id {id} no existe.");
        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InventoryMovement>> CreateMovement([FromBody] CreateInventoryMovementDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var created = await _movementRepo.CreateMovementAsync(dto);
            return CreatedAtRoute("GetInventoryMovement", new { id = created.IdInventoryMovement }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno al procesar el movimiento.", detail = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryMovement>> UpdateMovement(int id, [FromBody] UpdateInventoryMovementDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var updated = await _movementRepo.UpdateMovementAsync(id, dto);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al actualizar el movimiento.", detail = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMovement(int id)
    {
        var deleted = await _movementRepo.DeleteMovementAsync(id);
        if (!deleted) return NotFound($"Movimiento con Id {id} no encontrado.");
        return NoContent();
    }
}

