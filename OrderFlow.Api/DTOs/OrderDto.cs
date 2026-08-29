namespace OrderFlow.Api.DTOs
{
    public class OrderDto
    {
        public OrderDto(int id, int customerId, string status, List<OrderItemDto> items)
        {
            Id = id;
            CustomerId = customerId;
            Status = status;
            Items = items;
        }

        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Status { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
}
