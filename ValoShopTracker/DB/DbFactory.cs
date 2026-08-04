using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace ValoShopTracker.DB
{
    public class DbFactory : IDesignTimeDbContextFactory<DbConstructor>
    {
        public DbConstructor CreateDbContext(string[] args)
        {
            SQLitePCL.Batteries.Init();

            var optionsBuilder = new DbContextOptionsBuilder<DbConstructor>();
            var dbPath = Path.Combine(AppContext.BaseDirectory, "ValoShopTracker.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            return new DbConstructor(optionsBuilder.Options);
        }
    
    }
}
