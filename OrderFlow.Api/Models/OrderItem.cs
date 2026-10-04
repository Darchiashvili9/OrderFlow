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

        public OrderItem(int productId, decimal unitPrice, int quantity)
        {
            if (productId > 0)
                ProductId = productId;
            else throw new ValidationException("productId cant be 0");

            if (quantity > 0)
                Quantity = quantity;
            else throw new ValidationException("quantity must be more then 0");

            if (unitPrice > 0)
                UnitPrice = unitPrice;
            else throw new ValidationException("price cant be less than 1");
        }
    }
}
