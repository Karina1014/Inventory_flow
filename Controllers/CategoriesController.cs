using AutoMapper;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Flow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetCategories()
        {
            var categories = _categoryRepository.GetCategories();
            var categoriesDto = _mapper.Map<List<CategoryDto>>(categories);
            return Ok(categoriesDto);
        }

        [HttpGet("{IdCategory:int}", Name = "GetCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetCategory(int IdCategory)
        {
            var category = _categoryRepository.GetCategory(IdCategory);
            if (category == null)
            {
                return NotFound($"La categoría con el id {IdCategory} no existe");
            }
            var categoryDto = _mapper.Map<CategoryDto>(category);
            return Ok(categoryDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            if (createCategoryDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!_categoryRepository.IsUniqueName(createCategoryDto.Name))
            {
                return BadRequest("Ya existe una categoría con ese nombre.");
            }
            var result = await _categoryRepository.CreateCategory(createCategoryDto);
            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al crear la categoría");
            }
            return CreatedAtRoute("GetCategory", new { IdCategory = result.IdCategory }, result);
        }

        [HttpPut("{IdCategory:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCategory(int IdCategory, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            if (updateCategoryDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (_categoryRepository.GetCategory(IdCategory) == null)
            {
                return NotFound($"La categoría con el id {IdCategory} no existe");
            }
            await _categoryRepository.UpdateCategory(IdCategory, updateCategoryDto);
            return NoContent();
        }

        [HttpDelete("{IdCategory:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteCategory(int IdCategory)
        {
            if (!_categoryRepository.DeleteCategory(IdCategory))
            {
                return NotFound($"La categoría con el id {IdCategory} no existe");
            }
            return NoContent();
        }
    }
}
