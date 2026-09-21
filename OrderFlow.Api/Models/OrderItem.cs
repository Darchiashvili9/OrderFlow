using OrderFlow.Api.Exceptions;

namespace OrderFlow.Api.Models
{
    public class OrderItem
    {
        public int Id { get; private set; }
        public int Quantity { get; private set; }
        public int ProductId { get; private set; }
        public int OrderId { get; private set; }
        public decimal UnitPrice { get; private set; }

        public Order Order { get; private set; } = null!;
        public Product Product { get; private set; } = null!;

        private OrderItem() { }

        public OrderItem(Product product, int quantity)
        {
            if (product != null)
                Product = product;
            else throw new ValidationException("product cant be null");

            if (quantity > 0)
                Quantity = quantity;
            else throw new ValidationException("quantity must be more then 0");

            UnitPrice = product.ProductPrice;
        }


    }
}
