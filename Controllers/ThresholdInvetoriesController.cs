using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Flow.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ThresholdInvetoriesController : ControllerBase
{
    private readonly IThresholdInvetoryRepository _repo;
    private readonly IMapper _mapper;

    public ThresholdInvetoriesController(IThresholdInvetoryRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ICollection<ThresholdInvetoryDto>))]
    public async Task<ActionResult<ICollection<ThresholdInvetoryDto>>> GetThresholds()
    {
        var list = await _repo.GetThresholdsAsync();
        return Ok(_mapper.Map<List<ThresholdInvetoryDto>>(list));
    }

    [HttpGet("{id:int}", Name = "GetThresholdInvetory")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ThresholdInvetoryDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ThresholdInvetoryDto>> GetThreshold(int id)
    {
        var item = await _repo.GetThresholdByIdAsync(id);
        if (item == null) return NotFound($"Umbral con Id {id} no existe.");
        return Ok(_mapper.Map<ThresholdInvetoryDto>(item));
    }

    [HttpGet("current")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ThresholdInvetoryDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ThresholdInvetoryDto>> GetCurrentThreshold()
    {
        var item = await _repo.GetCurrentThresholdAsync();
        if (item == null) return NotFound("No hay un umbral configurado actualmente.");
        return Ok(_mapper.Map<ThresholdInvetoryDto>(item));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ThresholdInvetoryDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ThresholdInvetoryDto>> CreateThreshold([FromBody] CreateThresholdInvetoryDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var created = await _repo.CreateThresholdAsync(dto);
            var resultDto = _mapper.Map<ThresholdInvetoryDto>(created);
            return CreatedAtRoute("GetThresholdInvetory", new { id = resultDto.IdThresholdInvetory }, resultDto);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ThresholdInvetoryDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ThresholdInvetoryDto>> UpdateThreshold(int id, [FromBody] UpdateThresholdInvetoryDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var updated = await _repo.UpdateThresholdAsync(id, dto);
            return Ok(_mapper.Map<ThresholdInvetoryDto>(updated));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteThreshold(int id)
    {
        var deleted = await _repo.DeleteThresholdAsync(id);
        if (!deleted) return NotFound($"Umbral con Id {id} no encontrado.");
        return NoContent();
    }
}

