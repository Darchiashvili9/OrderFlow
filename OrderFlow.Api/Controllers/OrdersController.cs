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

        public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto dto)
        {
            var order = await _service.CreateAsync(dto.CustomerId, dto.Items);

            OrderDto oDto = new(
                order.Id, order.CustomerId, order.Status, order.OrderItems.Select(p => productId)

                );
        }
    }
}
