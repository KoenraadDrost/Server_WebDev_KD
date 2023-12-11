using Microsoft.EntityFrameworkCore;
using Setup.Models;
using System.Collections.Generic;

namespace Setup.Data
{
    public class ServerContext : DbContext
    {
        public ServerContext(DbContextOptions<ServerContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}