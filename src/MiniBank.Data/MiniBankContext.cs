using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using MiniBank.Core.BankAccounts;
using MiniBank.Core.TransactionsHistories;
using MiniBank.Core.Users;

namespace MiniBank.Core;

public class MiniBankContext : DbContext
{
    public DbSet<UserDbModel> Users { get; set; }
    public DbSet<BankAccountDbModel> BankAccounts { get; set; }
    public DbSet<TransactionsHistoryDbModel> Transactions { get; set; }
    
    public MiniBankContext(DbContextOptions options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MiniBankContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseNpgsql().UseSnakeCaseNamingConvention();

    public class Factory : IDesignTimeDbContextFactory<MiniBankContext>
    {
        public MiniBankContext CreateDbContext(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../MiniBank.Web"))
                .AddJsonFile("appsettings.json")
                .Build();
            
            var connectionString = config.GetConnectionString("DbConnectionString");
            
            var options = new DbContextOptionsBuilder()
                .UseNpgsql(connectionString)
                .Options;

            return new MiniBankContext(options);
        }
    }
}