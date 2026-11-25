using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;

namespace OrderFlow.Api.Tests.Infrastructure
{
    public abstract class DatabaseTestBase : IDisposable
    {
        private readonly DbContextOptions<OrderFlowDbContext> _options;

        protected DatabaseTestBase()
        {
            var dbName = $"OrderFlowTestDb_{Guid.NewGuid():N}";
            _options = new DbContextOptionsBuilder<OrderFlowDbContext>().UseSqlServer($@"Server=(localdb)\MSSQLLocalDB;Database={dbName};Trusted_Connection=True").Options;

            using var context = CreateContext();
            context.Database.Migrate();
        }

        protected OrderFlowDbContext CreateContext() => new(_options);

        public void Dispose()
        {
            using var context = CreateContext();
            context.Database.EnsureDeleted();
        }
    }
}
