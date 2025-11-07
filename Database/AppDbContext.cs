using FinanceTracker.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{ 
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<Notifications> Notifications => Set<Notifications>();
}
