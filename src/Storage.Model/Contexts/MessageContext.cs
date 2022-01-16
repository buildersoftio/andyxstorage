using Microsoft.EntityFrameworkCore;

namespace Buildersoft.Andy.X.Storage.Model.Contexts
{
    public class MessageContext : DbContext
    {
        private readonly string messagePartitionLocation;

        public MessageContext()
        {
            // For DI
        }

        public MessageContext(string consumerDbFileLocation)
        {
            this.messagePartitionLocation = consumerDbFileLocation;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={messagePartitionLocation}");
        }

        public DbSet<Entities.Message> Messages { get; set; }
    }
}
