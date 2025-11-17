using Microsoft.EntityFrameworkCore;

public class BudgetTrackerDbContext : DbContext
{
    public BudgetTrackerDbContext(DbContextOptions<BudgetTrackerDbContext> options)
        : base(options) 
    {
    }
    public DbSet<BudgetTransaction> BudgetTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BudgetTransaction>()
            .Property(b => b.Amount)
            .HasColumnType("decimal(18, 2)");

        base.OnModelCreating(modelBuilder);
    }
}





