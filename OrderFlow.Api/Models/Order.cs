using OrderFlow.Api.Exceptions;

namespace OrderFlow.Api.Models
{
    public class Order
    {
        public int Id { get; private set; }
        public int CustomerId { get; private set; }
        public DateTime OrderDate { get; private set; }
        public OrderStatus Status { get; private set; }
        public Customer Customer { get; private set; } = null!;
        public List<OrderItem> OrderItems { get; private set; } = new();

        private Order() { }

        public Order(Customer customer)
        {
            this.OrderDate = DateTime.Now;
            this.Status = OrderStatus.Pending;

            if (customer != null)
                this.Customer = customer;
            else throw new ValidationException("customer cant be null");
        }

        public void AddItem(int productId, decimal unitPrice, int quantity)
        {
            OrderItems.Add(new OrderItem(productId, unitPrice, quantity));
        }

        public void Cancel()
        {
            if (Status == OrderStatus.Canceled)
                throw new DomainException("it is already Canceled");

            if (this.Status != OrderStatus.Done)
                this.Status = OrderStatus.Canceled;
            
            else throw new DomainException("order is already completed, cant be canceled");
        }

        public void Complete()
        {
            if (Status == OrderStatus.Done)
                throw new DomainException("it is already completed");

            if (OrderItems.Count == 0)
                throw new DomainException("you need at least one product for order");

            if (Status != OrderStatus.Canceled)
                Status = OrderStatus.Done;

            else throw new DomainException("order is already canceled, cant be completed you need new order");
        }
    }
}
