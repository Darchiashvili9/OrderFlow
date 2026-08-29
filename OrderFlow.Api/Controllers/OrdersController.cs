using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Services;

namespace OrderFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _service;

        public OrdersController(OrderService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto dto)
        {
            var order = await _service.CreateAsync(dto.CustomerId, dto.Items);

            var itemDtos = order.OrderItems
                .Select(item => new OrderItemDto
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList();

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = order.Id },
                new OrderDto(order.Id, order.CustomerId, order.Status.ToString(),
                itemDtos));
        }

        [HttpGet("{iD}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            var order = await _service.GetByIdAsync(id);
            if (order is null)
                return NotFound();

            var orderItems = order.OrderItems.Select(
                  items => new OrderItemDto
                  {
                      ProductId = items.ProductId,
                      Quantity = items.Quantity,
                      UnitPrice = items.UnitPrice
                  }).ToList();

            return Ok(new OrderDto(order.Id, order.CustomerId, order.Status.ToString(), orderItems));
        }
    }
}
