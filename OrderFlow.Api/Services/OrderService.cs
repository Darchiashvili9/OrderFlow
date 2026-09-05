using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Exceptions;
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
                throw new NotFoundException($"Customer with ID {customerId} was not found.");

            Order order = new(customer);

            var groupedList = list
                .GroupBy(p => p.ProductId)
                .Select(group => new CreateOrderItemDto
                {
                    ProductId = group.Key,
                    Quantity = group.Sum(q => q.Quantity)

                }).ToList();

            var productIds = groupedList.Select(item => item.ProductId).Distinct().ToList();

            var products = await _context.Products.Where(prod => productIds.Contains(prod.Id)).ToListAsync();

            if (productIds.Count != products.Count)
                throw new NotFoundException("One or more products were not found");

            foreach (var item in groupedList)
            {
                var product = products.Single(prod => prod.Id == item.ProductId);
                order.AddItem(product, item.Quantity);
            }

            _context.Orders.Add(order);

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return order;
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (i == 2)
                        throw;

                    //reload products
                    foreach (var product in products)
                    {
                        await _context.Entry(product).ReloadAsync();
                    }

                    foreach (var item in groupedList)
                    {
                        var product = products.Single(prod => prod.Id == item.ProductId);
                        product.ReduceStock(item.Quantity);
                    }
                }
            }
            throw new InvalidOperationException("Concurrency retry loop exited unexpectedly.");
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

        public async Task<Order?> CompleteAsync(int orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId);

            if (order is null)
                return null;

            order.Complete();
            await _context.SaveChangesAsync();

            return order;
        }

        public async Task<Order?> CancelAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order is null)
                return null;

            order.Cancel();
            await _context.SaveChangesAsync();

            return order;
        }
    }
}
