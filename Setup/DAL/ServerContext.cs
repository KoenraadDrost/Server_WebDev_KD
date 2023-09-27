using Microsoft.EntityFrameworkCore;
using Setup.Models;

namespace Setup.DAL
{
    public class ServerContext : DbContext
    {
        public ServerContext(DbContextOptions<ServerContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
    }
}
