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
            Assert.Throws<ValidationException>(() => new Order(null!));
        }

        [Fact]
        public void AddItem_ValidValues_AddsItem()
        {
            Customer customer = new("testCustomer", "The Land", "BW@Deutschland.de");
            Order order = new(customer);
            order.AddItem(1, 100, 3);
            Assert.Single(order.OrderItems);
            Assert.Equal(3, order.OrderItems[0].Quantity);
            Assert.Equal(100m, order.OrderItems[0].UnitPrice);
            Assert.Equal(1, order.OrderItems[0].ProductId);
        }

        [Fact]
        public void Complete_ValidOrder_SetsStatusToDone()
        {
            Customer customer = new("testCustomer", "The Land", "BW@Deutschland.de");
            Order order = new(customer);
            order.AddItem(1, 100, 30);
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
            order.AddItem(1, 100, 30);
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
            order.AddItem(1, 100, 30);
            order.Cancel();

            Assert.Equal(OrderStatus.Canceled, order.Status);
        }

        [Fact]
        public void Cancel_AlreadyCanceled_ThrowsDomainException()
        {
            Customer customer = new("testCustomer", "The Land", "BW@Deutschland.de");
            Order order = new(customer);
            order.AddItem(1, 100, 30);
            order.Cancel();

            Assert.Throws<DomainException>(() => order.Cancel());
            Assert.Equal(OrderStatus.Canceled, order.Status);
        }

        [Fact]
        public void Cancel_AlreadyDone_ThrowsDomainException()
        {
            Customer customer = new("testCustomer", "The Land", "BW@Deutschland.de");
            Order order = new(customer);
            order.AddItem(1, 100, 30);
            order.Complete();

            Assert.Throws<DomainException>(() => order.Cancel());
            Assert.Equal(OrderStatus.Done, order.Status);
        }
    }
}
