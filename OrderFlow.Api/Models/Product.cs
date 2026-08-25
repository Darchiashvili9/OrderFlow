namespace OrderFlow.Api.Models
{
    public class Product
    {
        public int Id { get; private set; }
        public string ProductName { get; private set; }
        public int Stock { get; private set; }
        public decimal ProductPrice { get; private set; }
        public List<OrderItem> OrderItems { get; private set; } = new();


        private Product()
        {

        }
        public Product(string name, int stock, decimal price)
        {
            if (string.IsNullOrWhiteSpace(name) || price <= 0 || stock < 0)
                throw new InvalidDataException("data not valid");

            ProductName = name;
            Stock = stock;
            ProductPrice = price;
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidDataException("data not valid");

            ProductName = name;
        }

        public void ChangePrice(decimal price)
        {
            if (price <= 0)
                throw new InvalidOperationException("price cant be less then 1");

            ProductPrice = price;
        }

        public void AddStock(int quantity)
        {
            if (quantity < 1)
                throw new InvalidOperationException("quantity cant be less then 1");

            Stock += quantity;
        }

        public void ReduceStock(int quantity)
        {
            if (quantity < 1 || quantity > Stock)
                throw new InvalidOperationException("invalid number for reducing current stock");

            Stock -= quantity;
        }
    }
}