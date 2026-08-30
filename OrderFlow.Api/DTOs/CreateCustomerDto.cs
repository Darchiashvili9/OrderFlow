namespace OrderFlow.Api.DTOs
{
    public class CreateCustomerDto
    {
        public CreateCustomerDto(string name, string email, string address)
        {
            Name = name;
            Email = email;
            Address = address;
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }
}
