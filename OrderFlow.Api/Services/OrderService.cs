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

            var products = await _context.Products
                .Where(prod => productIds
                .Contains(prod.Id))
                .Select(prod => new
                {
                    prod.Id,
                    prod.ProductPrice
                })
                .ToListAsync();

            if (productIds.Count != products.Count)
                throw new NotFoundException("One or more products were not found");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in groupedList)
                {
                    var product = products.Single(prod => prod.Id == item.ProductId);

                    var affectedRows = await _context.Products
                        .Where(id => id.Id == item.ProductId && id.Stock >= item.Quantity)
                        .ExecuteUpdateAsync(prod => prod.SetProperty(p => p.Stock, p => p.Stock - item.Quantity));

                    if (affectedRows == 0)
                        throw new DomainException("No enough stock to make request");

                    order.AddItem(product.Id, product.ProductPrice, item.Quantity);
                }
                _context.Orders.Add(order);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order is null)
                return null;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in order.OrderItems)
                {
                    await _context.Products
                        .Where(id => id.Id == item.ProductId)
                        .ExecuteUpdateAsync(prod => prod.SetProperty(p => p.Stock, p => p.Stock + item.Quantity));
                }

                order.Cancel();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
