using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.Services;
using OrderFlow.Api.Tests.Infrastructure;

namespace OrderFlow.Api.Tests.Models.Integration
{
    public class CustomerServiceIntegrationTests: DatabaseTestBase
    {
        [Fact]
        public async Task CreateAsync_ValidCustomer_CreatesCustomer()
        {
            int customerId;
            {
                using var context = CreateContext();
                var service = new CustomerService(context);
                var custo = await service.CreateAsync("Name", "a@b.de", "Some Street 1");
                customerId = custo.Id;
            }

            {
                using var verificationContext = CreateContext();
                var customa = await verificationContext.Customers.SingleAsync(p => p.Id == customerId);
                Assert.Equal("Name", customa.Name);
                Assert.Equal("a@b.de", customa.Email);
                Assert.Equal("Some Street 1", customa.Address);
            }
        }
    }
}
