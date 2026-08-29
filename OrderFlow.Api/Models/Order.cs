namespace OrderFlow.Api.Models
{
    public class Order
    {
        public int Id { get; private set; }
        public int CustomerId { get; private set; }
        public DateTime OrderDate { get; private set; }
        public OrderStatus Status { get; private set; }
        public Customer Customer { get; private set; }
        public List<OrderItem> OrderItems { get; private set; } = new();

        private Order()
        {

        }

        public Order(Customer customer)
        {
            this.OrderDate = DateTime.Now;
            this.Status = OrderStatus.Pending;

            if (customer != null)
                this.Customer = customer;
            else throw new InvalidDataException("customer cant be null");
        }

        public void AddItem(Product product, int quantity)
        {
            if (product is null)
                throw new ArgumentNullException("product cant be null");

            else
            {
                product.ReduceStock(quantity);
                OrderItems.Add(new OrderItem(product, quantity));
            }
        }

        public void Cancel()
        {
            if (Status == OrderStatus.Canceled)
                throw new InvalidOperationException("it is already Canceled");

            if (this.Status != OrderStatus.Done)
            {
                foreach (var item in OrderItems)
                {
                    item.Product.AddStock(item.Quantity);
                }
                this.Status = OrderStatus.Canceled;
            }

            else throw new InvalidOperationException("order is already completed, cant be canceled");
        }

        public void Complete()
        {
            if (Status == OrderStatus.Done)
                throw new InvalidOperationException("it is already completed");

            if (OrderItems.Count == 0)
                throw new InvalidOperationException("you need at least one product for order");

            if (Status != OrderStatus.Canceled)
                Status = OrderStatus.Done;

            else throw new InvalidOperationException("order is already canceled, cant be completed you need new order");
        }
    }
}
