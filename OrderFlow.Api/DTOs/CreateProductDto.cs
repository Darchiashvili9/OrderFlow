namespace OrderFlow.Api.DTOs
{
    public class CreateProductDto
    {
        public required string Name { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
    }
}
