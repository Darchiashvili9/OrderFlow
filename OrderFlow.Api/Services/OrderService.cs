using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Services
{
    public class OrderService
    {
        private readonly OrderFlowDbContext _context;

        public OrderService(OrderFlowDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateAsync(int customerId, List<CreateOrderItemDto> list)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer is null)
                throw new InvalidOperationException($"Customer with ID {customerId} was not found.");

            Order order = new(customer);

            var productIds = list.Select(item => item.ProductId).Distinct().ToList();

            var products = await _context.Products.Where(prod => productIds.Contains(prod.Id)).ToListAsync();

            if (productIds.Count != products.Count)
                throw new InvalidOperationException("product cant be found");

            foreach (var item in list)
            {
                var product = products.Single(prod => prod.Id == item.ProductId);
                order.AddItem(product, item.Quantity);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order?> GetByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(items => items.OrderItems)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
