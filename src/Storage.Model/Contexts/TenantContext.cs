using Microsoft.EntityFrameworkCore;

namespace Buildersoft.Andy.X.Storage.Model.Contexts
{
    public class TenantContext : DbContext
    {
        private readonly string consumerDbFileLocation;

        public TenantContext()
        {
            // For DI
        }

        public TenantContext(string consumerDbFileLocation)
        {
            this.consumerDbFileLocation = consumerDbFileLocation;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={consumerDbFileLocation}");
        }

        public DbSet<Entities.ConsumerMessage> ConsumerMessages { get; set; }

    }
}
