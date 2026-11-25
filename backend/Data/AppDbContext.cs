using Microsoft.EntityFrameworkCore;
using RubinoDigital.Api.Models;

namespace RubinoDigital.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<Enquiry> Enquiries => Set<Enquiry>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Quote>().HasIndex(q => q.Reference).IsUnique();
        modelBuilder.Entity<Enquiry>().HasIndex(e => e.Reference).IsUnique();
        modelBuilder.Entity<RefreshToken>()
            .HasOne(r => r.User).WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Quote>()
            .HasOne(q => q.User).WithMany(u => u.Quotes)
            .HasForeignKey(q => q.UserId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Enquiry>()
            .HasOne(e => e.User).WithMany(u => u.Enquiries)
            .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}
