namespace OrderFlow.Api.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }

        public ProductDto(int id, string name, int stock, decimal price)
        {
            Id = id;
            Name = name;
            Stock = stock;
            Price = price;
        }
    }
}
