using OrderFlow.Api.Exceptions;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Tests.Models
{
    public class OrderItemsTests
    {
        [Fact]
        public void CreateOrderItem_ValidConstruction_CreatesOrderItem()
        {
            var product = new Product("testProduct", 10, 7);
            var item = new OrderItem(product, 5);

            Assert.Equal(5, item.Quantity);
            Assert.Equal("testProduct", item.Product.ProductName);
            Assert.Equal(7, item.UnitPrice);
        }

        [Fact]
        public void CreateOrderItem_ProductIsNull_ThrowsValidationException()
        {
            Assert.Throws<ValidationException>(() => new OrderItem(null, 5));
        }

        [Fact]
        public void CreateOrderItem_QuantityLessThanOne_ThrowsValidationException()
        {
            var product = new Product("testProduct", 10, 7);
            Assert.Throws<ValidationException>(() => new OrderItem(product, 0));

        }

    }
}
