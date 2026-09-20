using OrderFlow.Api.Exceptions;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Tests.Models
{
    public class OrderTests
    {
        [Fact]
        public void CreateOrder_ValidCustomer_CreatesOrder()
        {
            Customer customer = new("testCustomer", "The Land", "BW@Deutschland.de");
            Order order = new(customer);

            Assert.Equal(OrderStatus.Pending, order.Status);
            Assert.Same(customer, order.Customer);
            Assert.Empty(order.OrderItems);
        }

        [Fact]
        public void CreateOrder_CustomerIsNull_ThrowsValidationException()
        {
            Assert.Throws<ValidationException>(() => new Order(null));
        }

        [Fact]
        public void AddItem_ValidProductAndQuantity_AddsItemAndReducesStock()
        {
            Customer customer = new("testCustomer", "The Land", "BW@Deutschland.de");
            Order order = new(customer);
            var product = new Product("testProduct", 10, 100);
            order.AddItem(product, 3);
            Assert.Single(order.OrderItems);
            Assert.Equal(3, order.OrderItems[0].Quantity);
            Assert.Equal(100m, order.OrderItems[0].UnitPrice);
            Assert.Same(product, order.OrderItems[0].Product);
            Assert.Equal(7, product.Stock);
        }

        [Fact]
        public void AddItem_InsufficientStock_ThrowsDomainException()
        {
            Customer customer = new("testCustomer", "The Land", "BW@Deutschland.de");
            Order order = new(customer);
            var product = new Product("testProduct", 10, 100);

            Assert.Throws<DomainException>(() => order.AddItem(product, 11));
            Assert.Equal(10, product.Stock);
            Assert.Empty(order.OrderItems);
        }

        [Fact]
        public void AddItem_QuantityIsLessThanOne_ThrowsDomainException()
        {
            Customer customer = new("testCustomer", "The Land", "BW@Deutschland.de");
            Order order = new(customer);
            var product = new Product("testProduct", 10, 100);

            Assert.Throws<ValidationException>(() => order.AddItem(product, 0));
            Assert.Equal(10, product.Stock);
            Assert.Empty(order.OrderItems);
        }

        [Fact]
        public void AddItem_ProductIsNull_ThrowsValidationException()
        {
            Customer customer = new("testCustomer", "The Land", "BW@Deutschland.de");
            Order order = new(customer);

            Assert.Throws<ValidationException>(() => order.AddItem(null, 5));
            Assert.Empty(order.OrderItems);
        }

        [Fact]
        public void Complete_ValidOrder_SetsStatusToDone()
        {
            Customer customer = new("testCustomer", "The Land", "BW@Deutschland.de");
            Order order = new(customer);
            var product = new Product("testProduct", 10, 100);
            order.AddItem(product, 3);
            order.Complete();

            Assert.Equal(OrderStatus.Done, order.Status);
        }

        [Fact]
        public void Complete_OrderHasNoItems_ThrowsDomainException()
        {
            Customer customer = new("testCustomer", "The Land", "BW@Deutschland.de");
            Order order = new(customer);

            Assert.Throws<DomainException>(() => order.Complete());
            Assert.Equal(OrderStatus.Pending, order.Status);
        }

        [Fact]
        public void Complete_OrderAlreadyCompleted_ThrowsDomainException()
        {
            Customer customer = new("testCustomer", "The Land", "BW@Deutschland.de");
            Order order = new(customer);
            var product = new Product("testProduct", 10, 100);
            order.AddItem(product, 3);
            order.Complete();

            Assert.Throws<DomainException>(() => order.Complete());
            Assert.Equal(OrderStatus.Done, order.Status);
        }

        [Fact]
        public void Complete_OrderCanceled_ThrowsDomainException()
        {
            Customer customer = new("testCustomer", "The Land", "BW@Deutschland.de");
            Order order = new(customer);
            order.Cancel();

            Assert.Throws<DomainException>(() => order.Complete());
            Assert.Equal(OrderStatus.Canceled, order.Status);
        }

        [Fact]
        public void Cancel_ValidOrder_SetsStatusToCanceled()
        {
            Customer customer = new("testCustomer", "The Land", "BW@Deutschland.de");
            Order order = new(customer);
            var product = new Product("testProduct", 10, 100);
            order.AddItem(product, 3);
            order.Cancel();

            Assert.Equal(OrderStatus.Canceled, order.Status);
            Assert.Equal(10, product.Stock);
        }

        [Fact]
        public void Cancel_AlreadyCanceled_ThrowsDomainException()
        {
            Customer customer = new("testCustomer", "The Land", "BW@Deutschland.de");
            Order order = new(customer);
            var product = new Product("testProduct", 10, 100);
            order.AddItem(product, 3);
            order.Cancel();

            Assert.Throws<DomainException>(() => order.Cancel());
            Assert.Equal(OrderStatus.Canceled, order.Status);
            Assert.Equal(10, product.Stock);
        }

        [Fact]
        public void Cancel_AlreadyDone_ThrowsDomainException()
        {
            Customer customer = new("testCustomer", "The Land", "BW@Deutschland.de");
            Order order = new(customer);
            var product = new Product("testProduct", 10, 100);
            order.AddItem(product, 3);
            order.Complete();

            Assert.Throws<DomainException>(() => order.Cancel());
            Assert.Equal(OrderStatus.Done, order.Status);
            Assert.Equal(7, product.Stock);
        }
    }
}
