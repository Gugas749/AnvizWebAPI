namespace AnvizWebSDK
{
    using AnvizWebSDK.Models;
    using Microsoft.EntityFrameworkCore;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        public DbSet<Device> Devices { get; set; }
    }
}
