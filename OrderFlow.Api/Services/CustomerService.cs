using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Services
{
    public class CustomerService
    {
        private readonly OrderFlowDbContext _context;

        public CustomerService(OrderFlowDbContext context)
        {
            _context = context;
        }

        public async Task<Customer> CreateAsync(string name, string email, string address)
        {
            Customer newCustomer = new(name, email, address);

            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();

            return newCustomer;
        }

        public async Task<Customer?> GetByIdAsync(int customerId)
        {
            return await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == customerId);
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            return await _context.Customers.AsNoTracking().ToListAsync();
        }
    }
}
