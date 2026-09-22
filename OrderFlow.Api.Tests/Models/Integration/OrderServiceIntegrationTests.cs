using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Exceptions;
using OrderFlow.Api.Models;
using OrderFlow.Api.Services;

namespace OrderFlow.Api.Tests.Models.Integration
{
    public class OrderServiceIntegrationTests
    {
        private readonly DbContextOptions<OrderFlowDbContext> _options;

        public OrderServiceIntegrationTests()
        {
            var connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=OrderFlowTestDb;Trusted_Connection=True";
            var optionsBuilder = new DbContextOptionsBuilder<OrderFlowDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            _options = optionsBuilder.Options;
            ResetDatabase();
        }

        private OrderFlowDbContext CreateContext()
        {
            return new OrderFlowDbContext(_options);
        }

        private void ResetDatabase()
        {
            using var context = CreateContext();
            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }

        [Fact]
        public async Task CreateAsync_ValidOrder_CreatesOrder()
        {
            int customerId;
            int productId;
            int orderId;

            {
                using var context = CreateContext();
                var customer = new Customer("testName", "The Land", "BW@Deutschland.de");
                var product = new Product("testProduct", 10, 90);
                await context.Customers.AddAsync(customer);
                await context.Products.AddAsync(product);
                await context.SaveChangesAsync();
                customerId = customer.Id;
                productId = product.Id;
            }

            {
                using var actingContext = CreateContext();
                var service = new OrderService(actingContext);
                var order = await service.CreateAsync(customerId, [new CreateOrderItemDto { ProductId = productId, Quantity = 3 }]);
                orderId = order.Id;
            }

            {
                using var verificationContext = CreateContext();
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

        [Fact]
        public async Task CreateAsync_CustomerDoesNotExist_ThrowsNotFoundException()
        {
            using var context = CreateContext();
            OrderService service = new(context);
            await Assert.ThrowsAsync<NotFoundException>(async () => await service.CreateAsync(99999, []));
        }
    }
}
