using AutoMapper;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Flow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductsController(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProducts()
        {
            var products = _productRepository.GetProducts();
            var productsDto = _mapper.Map<List<ProductDto>>(products);
            return Ok(productsDto);
        }

        [HttpGet("{IdProduct:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProduct(int IdProduct)
        {
            var product = _productRepository.GetProduct(IdProduct);
            if (product == null)
            {
                return NotFound($"El producto con el id {IdProduct} no existe");
            }
            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (createProductDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!_productRepository.IsUniqueName(createProductDto.Name))
            {
                return BadRequest("Ya existe un producto con ese nombre.");
            }
            var result = await _productRepository.CreateProduct(createProductDto);
            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al crear el producto");
            }
            return CreatedAtRoute("GetProduct", new { IdProduct = result.IdProduct }, result);
        }

        [HttpPut("{IdProduct:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProduct(int IdProduct, [FromBody] UpdateProductDto updateProductDto)
        {
            if (updateProductDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (_productRepository.GetProduct(IdProduct) == null)
            {
                return NotFound($"El producto con el id {IdProduct} no existe");
            }
            await _productRepository.UpdateProduct(IdProduct, updateProductDto);
            return NoContent();
        }

        [HttpDelete("{IdProduct:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteProduct(int IdProduct)
        {
            if (!_productRepository.DeleteProduct(IdProduct))
            {
                return NotFound($"El producto con el id {IdProduct} no existe");
            }
            return NoContent();
        }
    }
}
