using Microsoft.AspNetCore.Mvc;
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

        [Fact]
        public async Task CreateAsync_ProductDoesNotExist_ThrowsNotFoundException()
        {
            int custId;
            {
                using var context = CreateContext();
                Customer cust = new("testName", "Deutschland", "testMail@mail.de");
                context.Customers.Add(cust);
                await context.SaveChangesAsync();
                custId = cust.Id;
            }

            {
                using var actingContext = CreateContext();
                OrderService service = new(actingContext);
                await Assert.ThrowsAsync<NotFoundException>(async () => await service.CreateAsync(custId, [new CreateOrderItemDto() { Quantity = 3, ProductId = 9999 }]));
            }
        }

        [Fact]
        public async Task CreateAsync_InsufficientStock_ThrowsDomainException()
        {
            int custId;
            int prodId;
            {
                using var context = CreateContext();
                Customer cust = new("testName", "Deutschland", "testMail@mail.de");
                Product prod = new("testProduct", 2, 5);
                context.Customers.Add(cust);
                context.Products.Add(prod);
                await context.SaveChangesAsync();
                custId = cust.Id;
                prodId = prod.Id;
            }

            {
                using var actingContext = CreateContext();
                OrderService service = new(actingContext);
                await Assert.ThrowsAsync<DomainException>(
                    async () => await service.CreateAsync(
                        custId,
                        [new CreateOrderItemDto() { Quantity = 3, ProductId = prodId }]
                    )
                );
            }

            {
                using var persistedContext = CreateContext();
                var product = await persistedContext.Products.SingleAsync(p => p.Id == prodId);
                Assert.Equal(2, product.Stock);
            }
        }

        [Fact]
        public async Task CreateAsync_DuplicateProductIds_GroupsQuantities()
        {
            int custId;
            int prodId;
            int orderId;
            {
                using var context = CreateContext();
                Customer cust = new("testName", "Deutschland", "testMail@mail.de");
                Product prod = new("testProduct", 30, 50);
                context.Customers.Add(cust);
                context.Products.Add(prod);
                await context.SaveChangesAsync();
                custId = cust.Id;
                prodId = prod.Id;
            }

            {
                using var actingContext = CreateContext();
                OrderService service = new(actingContext);

                List<CreateOrderItemDto> ord = new()
                {
                    new CreateOrderItemDto() {ProductId = prodId, Quantity = 5},
                    new CreateOrderItemDto() { ProductId = prodId, Quantity = 10 }
                };
                var order = await service.CreateAsync(custId, ord);
                orderId = order.Id;
            }

            {
                using var verificationContext = CreateContext();
                var product = await verificationContext.Products.SingleAsync(p => p.Id == prodId);
                var orderItems = await verificationContext.OrderItems.Where(ord => ord.OrderId == orderId).ToListAsync();

                Assert.Equal(15, product.Stock);
                Assert.Single(orderItems);
                Assert.Equal(15, orderItems[0].Quantity);
            }
        }

        [Fact]
        public async Task CompleteAsync_PendingOrder_SetsStatusToDone()
        {
            int custId;
            int prodId;
            int orderId;
            {
                using var context = CreateContext();
                Customer cust = new("testName", "Deutschland", "testMail@mail.de");
                Product prod = new("testProduct", 30, 50);
                context.Customers.Add(cust);
                context.Products.Add(prod);
                await context.SaveChangesAsync();
                custId = cust.Id;
                prodId = prod.Id;
            }

            {
                using var actingContext = CreateContext();
                OrderService service = new(actingContext);

                List<CreateOrderItemDto> ord = new()
                {
                    new CreateOrderItemDto() {ProductId = prodId, Quantity = 5},
                };
                var order = await service.CreateAsync(custId, ord);
                orderId = order.Id;
            }

            {
                using var completeContext = CreateContext();
                OrderService service = new(completeContext);
                await service.CompleteAsync(orderId);
            }

            {
                using var verificationContext = CreateContext();
                var order = await verificationContext.Orders.SingleAsync(o => o.Id == orderId);
                Assert.Equal(OrderStatus.Done, order.Status);
            }
        }

        [Fact]
        public async Task CancelAsync_PendingOrder_RestoresStockAndSetsStatusToCanceled()
        {


        }
    }
}
