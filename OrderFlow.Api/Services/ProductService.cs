using Microsoft.EntityFrameworkCore;
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

        public async Task<List<Product>> GetAllAsync()
        {
            return await _dbContext.Products
                .AsNoTracking()
                .ToListAsync();
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

                else throw new ValidationException("restock exceeds stock limit");
            }
        }
    }
}
