using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Services;

namespace OrderFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _service;

        public ProductsController(ProductService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto product)
        {
            var added = await _service.CreateAsync(product.Name, product.Stock, product.Price);
            ProductDto productDto = new(added.Id, added.ProductName, added.Stock, added.ProductPrice);

            return CreatedAtAction(nameof(GetProductById), new { id = productDto.Id }, productDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProductById(int id)
        {
            var prod = await _service.GetByIdAsync(id);

            if (prod is null)
                return NotFound();

            ProductDto product = new(prod.Id, prod.ProductName, prod.Stock, prod.ProductPrice);
            return product;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductDto>>> GetAll()
        {
            var list = await _service.GetAllAsync();
            List<ProductDto> dtoList = list.Select(o => new ProductDto(o.Id, o.ProductName, o.Stock, o.ProductPrice)).ToList();
            return dtoList;
        }

        [HttpPut("{id}/price")]
        public async Task<ActionResult> UpdateProductPrice(int id, decimal price)
        {
            var hasChanged = await _service.ChangePriceAsync(id, price);

            if (!hasChanged)
                return NotFound();
            else return NoContent();
        }

    }
}
