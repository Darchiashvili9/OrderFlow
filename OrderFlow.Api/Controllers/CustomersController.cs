using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Common;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Services;
using System.Net;

namespace OrderFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly CustomerService _service;

        public CustomersController(CustomerService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto customer)
        {
            var result = await _service.CreateAsync(customer.Name, customer.Email, customer.Address);

            return CreatedAtAction(
                nameof(GetCustomerById),
                new { id = result.Id },
                new CustomerDto(result.Id, result.Name, result.Email, result.Address));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomerById(int id)
        {
            var customer = await _service.GetByIdAsync(id);

            if (customer is null)
                return NotFound();

            return Ok(new CustomerDto(customer.Id, customer.Name, customer.Email, customer.Address));
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll([FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var list = await _service.GetAllAsync(page, pageSize, search);
            List<CustomerDto> cDto = list.Items.Select(c => new CustomerDto(c.Id, c.Name, c.Email, c.Address)).ToList();

            return Ok(new PagedResult<CustomerDto>(cDto, list.TotalCount, list.Page, list.PageSize, list.TotalPages));
        }
    }
}
