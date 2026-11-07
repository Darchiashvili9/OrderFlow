using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Common;
using OrderFlow.Api.Data;
using OrderFlow.Api.Models;
using System.ComponentModel.DataAnnotations;

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

        public async Task<PagedResult<Customer>> GetAllAsync(int page, int pageSize, string? search)
        {
            var offeset = PaginationHelper.CalculateOffset(page, pageSize);

            IQueryable<Customer> query = _context.Customers;

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(custo => custo.Name.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var totalPges = PaginationHelper.CalculateTotalPages(totalCount, pageSize);

            var res = await query
                .OrderByDescending(custo => custo.Id)
                .Skip(offeset)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<Customer>(res, totalCount, page, pageSize, totalPges);
        }
    }
}
