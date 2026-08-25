namespace OrderFlow.Api.Models
{
    public class Customer
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string Email { get; private set; }

        public List<Order> Orders { get; private set; } = new();
        public Customer(string name, string address, string email)
        {
            Name = name;
            Address = address;
            Email = email;
        }

        public void AddOrder(Order order)
        {
            if (order is null)
                throw new ArgumentNullException("order cant be null");

            Orders.Add(order);
        }
    }
}
