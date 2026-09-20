using OrderFlow.Api.Exceptions;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Tests.Models
{
    public class ProductTests
    {
        [Fact]
        public void ReduceStock_ValidQuantity_ReducesStock()
        {
            var product = new Product("testProduct", 300, 9);
            product.ReduceStock(10);
            Assert.Equal(290, product.Stock);
        }

        [Fact]
        public void ReduceStock_QuantityLessThanOne_ThrowsValidationException()
        {
            var product = new Product("testProduct", 300, 9);
            Assert.Throws<ValidationException>(() => product.ReduceStock(0));
        }

        [Fact]
        public void ReduceStock_QuantityExceedsStock_ThrowsDomainException()
        {
            var product = new Product("testProduct", 300, 9);
            Assert.Throws<DomainException>(() => product.ReduceStock(500));
        }

        [Fact]
        public void AddStock_ValidQuantity_AddsStock()
        {
            var product = new Product("testProduct", 300, 9);
            product.AddStock(50);
            Assert.Equal(350, product.Stock);
        }

        [Fact]
        public void CreateProduct_ValidConstruction_CreatesProduct()
        {
            var product = new Product("testProduct", 300, 9);
            Assert.Equal(300, product.Stock);
            Assert.Equal("testProduct", product.ProductName);
            Assert.Equal(9m, product.ProductPrice);
        }

    }
}
