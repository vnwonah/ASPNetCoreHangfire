using Microsoft.EntityFrameworkCore;
using AspNetCoreHangfire.Models;

namespace AspNetCoreHangfire.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
          : base(options)
        {

        }
        public DbSet<AspNetCoreHangfire.Models.Todo> Todo { get; set; }
    }
}
