using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Common;
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
            return Ok(product);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ProductDto>>> GetAll([FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var list = await _service.GetAllAsync(page, pageSize, search);
            List<ProductDto> dtoList = list.Items.Select(o => new ProductDto(o.Id, o.ProductName, o.Stock, o.ProductPrice)).ToList();

            return Ok(new PagedResult<ProductDto>(dtoList, list.TotalCount, list.Page, list.PageSize, list.TotalPages));
        }

        [HttpPut("{id}/price")]
        public async Task<ActionResult> UpdateProductPrice(int id, decimal price)
        {
            var hasChanged = await _service.ChangePriceAsync(id, price);

            if (!hasChanged)
                return NotFound();
            else return NoContent();
        }

        [HttpPost("{id}/restock")]
        public async Task<ActionResult> ReStock(int id, RestockRequestDto req)
        {
            var hasChanged = await _service.ReStockAsync(id, req.Quantity);

            if (!hasChanged)
                return NotFound();
            else return NoContent();
        }

    }
}
