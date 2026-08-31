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
                .Select(item => new OrderItemDto(
                    item.ProductId,
                    item.Quantity,
                    item.UnitPrice)).ToList();

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = order.Id },
                new OrderDto(order.Id, order.CustomerId, order.Status.ToString(),
                itemDtos));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            var order = await _service.GetByIdAsync(id);
            if (order is null)
                return NotFound();

            var orderItems = order.OrderItems.Select(
                  items => new OrderItemDto(
                      items.ProductId,
                      items.Quantity,
                      items.UnitPrice)).ToList();

            return Ok(new OrderDto(order.Id, order.CustomerId, order.Status.ToString(), orderItems));
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> GetAll()
        {
            var list = await _service.GetAllAsync();
            var orderDto = list.Select(
                o => new OrderDto(
                    o.Id,
                    o.CustomerId,
                    o.Status.ToString(),
                    o.OrderItems.Select(
                        item => new OrderItemDto(
                            item.ProductId,
                            item.Quantity,
                            item.UnitPrice)).ToList())).ToList();

            return Ok(orderDto);
        }

        [HttpPut("{id}/complete")]
        public async Task<ActionResult> CompleteOrder(int id)
        {
            var order = await _service.CompleteAsync(id);
            if (order is null)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{id}/cancel")]
        public async Task<ActionResult> CancelOrder(int id)
        {
            var order = await _service.CancelAsync(id);

            if (order is null)
                return NotFound();

            return NoContent();
        }
    }
}
