using Microsoft.EntityFrameworkCore;

namespace NationalInstruments
{
    public class TorpedoContext : DbContext
    {
        public DbSet<Outcome> Outcomes { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Stat> Stats { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseSqlServer(@"Database=Torpedo2;Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }
    }
}
