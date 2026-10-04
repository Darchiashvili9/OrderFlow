using OrderFlow.Api.Exceptions;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Tests.Models
{
    public class OrderItemsTests
    {
        [Fact]
        public void CreateOrderItem_ValidConstruction_CreatesOrderItem()
        {
            var item = new OrderItem(10, 7, 5);

            Assert.Equal(10, item.ProductId);
            Assert.Equal(5, item.Quantity);
            Assert.Equal(7, item.UnitPrice);
        }

        [Fact]
        public void CreateOrderItem_ProductIdIsLessThanOne_ThrowsValidationException()
        {
            Assert.Throws<ValidationException>(() => new OrderItem(0, 5, 5));
        }

        [Fact]
        public void CreateOrderItem_ProductPriceIsLessThanOne_ThrowsValidationException()
        {
            Assert.Throws<ValidationException>(() => new OrderItem(1, 0, 5));
        }

        [Fact]
        public void CreateOrderItem_QuantityLessThanOne_ThrowsValidationException()
        {
            Assert.Throws<ValidationException>(() => new OrderItem(10, 7, 0));
        }
    }
}
