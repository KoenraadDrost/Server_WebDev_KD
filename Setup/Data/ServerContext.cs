using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Setup.Models;
using System.Collections.Generic;

namespace Setup.Data
{
    public class ServerContext : IdentityDbContext
    {
        public ServerContext(DbContextOptions<ServerContext> options) : base(options)
        {
        }

        public DbSet<ChatUser> Users { get; set; }
    }
}