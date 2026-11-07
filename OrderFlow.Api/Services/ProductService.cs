using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Common;
using OrderFlow.Api.Data;
using OrderFlow.Api.Exceptions;
using OrderFlow.Api.Models;
using System.Collections;

namespace OrderFlow.Api.Services
{
    public class ProductService
    {
        private readonly OrderFlowDbContext _dbContext;

        public ProductService(OrderFlowDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Product> CreateAsync(string name, int stock, decimal price)
        {
            Product newProduct = new(name, stock, price);

            _dbContext.Products.Add(newProduct);
            await _dbContext.SaveChangesAsync();

            return newProduct;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _dbContext.Products.FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<PagedResult<Product>> GetAllAsync(int page, int pageSize, string? search)
        {
            var offset = PaginationHelper.CalculateOffset(page, pageSize);

            IQueryable<Product> query = _dbContext.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(prod => prod.ProductName.Contains(search));
            }

            var totalCount = await query.CountAsync();
            int totalPages = PaginationHelper.CalculateTotalPages(totalCount, pageSize);

            var res = await query
                .OrderByDescending(prod => prod.Id)
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Product>(res, totalCount, page, pageSize, totalPages);
        }

        public async Task<bool> ChangePriceAsync(int id, decimal newPrice)
        {
            var product = await _dbContext.Products.FindAsync(id);

            if (product is null)
                return false;

            product.ChangePrice(newPrice);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReStockAsync(int id, int quantity)
        {
            if (quantity < 1)
                throw new ValidationException("quantity is less than 1");

            var effectedRows = await _dbContext.Products
                .Where(prod => prod.Id == id && (int.MaxValue - quantity) >= prod.Stock)
                .ExecuteUpdateAsync(set => set.SetProperty(prop => prop.Stock, prop => prop.Stock + quantity));

            if (effectedRows == 1)
                return true;
            else
            {
                var prod = await _dbContext.Products.FindAsync(id);

                if (prod is null)
                    return false;

                else throw new DomainException("restock exceeds stock limit");
            }
        }
    }
}
