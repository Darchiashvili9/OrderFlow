using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Models;
using OrderFlow.Api.Services;

namespace OrderFlow.Api.Tests.Models.Integration
{
    public class OrderServiceIntegrationTests
    {
        [Fact]
        public async Task CreateAsync_ValidOrder_CreatesOrder()
        {
            var connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=OrderFlowTestDb;Trusted_Connection=True";
            var optionsBuilder = new DbContextOptionsBuilder<OrderFlowDbContext>();
            optionsBuilder.UseSqlServer(connectionString: connectionString);
            int customerId;
            int productId;
            int orderId;
            var options = optionsBuilder.Options;
            {
                using var context = new OrderFlowDbContext(options);
                context.Database.EnsureDeleted();
                context.Database.Migrate();

                var customer = new Customer("testName", "The Land", "BW@Deutschland.de");
                var product = new Product("testProduct", 10, 90);
                await context.Customers.AddAsync(customer);
                await context.Products.AddAsync(product);
                await context.SaveChangesAsync();
                customerId = customer.Id;
                productId = product.Id;
            }

            {
                using var actingContext = new OrderFlowDbContext(options);
                var service = new OrderService(actingContext);
                var order = await service.CreateAsync(customerId, [new CreateOrderItemDto { ProductId = productId, Quantity = 3 }]);
                orderId = order.Id;
            }

            {
                using var verificationContext = new OrderFlowDbContext(options);
                var productFromDb = await verificationContext.Products.SingleAsync(p => p.Id == productId);
                var orderFromDb = await verificationContext.Orders.Include(items => items.OrderItems).SingleAsync(o => o.Id == orderId);

                Assert.Equal(7, productFromDb.Stock);
                Assert.Equal(orderId, orderFromDb.Id);
                Assert.Single(orderFromDb.OrderItems);
                Assert.Equal(customerId, orderFromDb.CustomerId);
                Assert.Equal(OrderStatus.Pending, orderFromDb.Status);
                Assert.Equal(productId, orderFromDb.OrderItems[0].ProductId);
                Assert.Equal(3, orderFromDb.OrderItems[0].Quantity);
                Assert.Equal(90m, orderFromDb.OrderItems[0].UnitPrice);
            }
        }
    }
}
