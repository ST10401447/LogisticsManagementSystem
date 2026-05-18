using LogisticsManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LogisticsManagementSystem.Data
{
    public class LogisticDbContext: DbContext
    {
        public LogisticDbContext(DbContextOptions<LogisticDbContext> options) : base(options)
        {
        }
        public DbSet<Models.Client> Clients { get; set; }
        public DbSet<Models.Contract> Contracts { get; set; }
        public DbSet<Models.ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Contract>()
                .HasMany(c => c.ServiceRequest)
                .WithOne(s => s.Contract)
                .HasForeignKey(s => s.ContractID);
        }
    }
}
