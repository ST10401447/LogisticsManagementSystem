using Microsoft.EntityFrameworkCore;
using LogisticsManagementSystem.Api.Models;

namespace LogisticsManagementSystem.Api.Data
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
