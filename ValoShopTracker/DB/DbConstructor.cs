using Microsoft.EntityFrameworkCore;



namespace ValoShopTracker.DB
{
    public class DbConstructor :DbContext
    {
        public DbConstructor(DbContextOptions<DbConstructor> options) : base(options)
        {
        
        }
    
        public DbSet<User> Users { get; set; }
    }
}

