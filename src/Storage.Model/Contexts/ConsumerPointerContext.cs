using Microsoft.EntityFrameworkCore;

namespace Buildersoft.Andy.X.Storage.Model.Contexts
{
    public class ConsumerPointerContext : DbContext
    {
        private readonly string consumerDbFileLocation;

        public ConsumerPointerContext()
        {
            // For DI
        }

        public ConsumerPointerContext(string consumerDbFileLocation)
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
