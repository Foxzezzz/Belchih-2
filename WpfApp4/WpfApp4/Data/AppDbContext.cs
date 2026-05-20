using WpfApp4.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL; 

namespace WpfApp4.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<CloudFile> Files { get; set; }
        public DbSet<AccessRight> AccessRights { get; set; }
        public DbSet<ActionLog> ActionLogs { get; set; }
        public DbSet<SharedLink> SharedLinks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Database=postgres;Username=postgres;Password=123");
            }
        }
    }
}