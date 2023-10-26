using BankingPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankingPoc.Data;

public class BankingContext: DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }
    
    public BankingContext (DbContextOptions<BankingContext> options)
        : base(options)
    {
    }
    
    public BankingContext ()
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(p => p.Id)
            .ValueGeneratedOnAdd();
        
        modelBuilder.Entity<Account>()
            .Property(p => p.Id)
            .ValueGeneratedOnAdd();
    }
}