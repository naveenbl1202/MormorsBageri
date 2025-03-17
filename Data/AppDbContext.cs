using Microsoft.EntityFrameworkCore;
using MormorsBageri.Models;

namespace MormorsBageri.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Användare> Användare { get; set; }
        public DbSet<Butik> Butiker { get; set; }
        public DbSet<Produkt> Produkter { get; set; }
        public DbSet<Beställning> Beställningar { get; set; }
        public DbSet<Beställningsdetalj> Beställningsdetaljer { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Beställning>()
                .HasMany(b => b.Beställningsdetaljer)
                .WithOne()
                .HasForeignKey(bd => bd.BeställningId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Beställning>()
                .HasOne(b => b.Butik)
                .WithMany()
                .HasForeignKey(b => b.ButikId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Beställningsdetalj>()
                .HasOne(bd => bd.Produkt)
                .WithMany()
                .HasForeignKey(bd => bd.ProduktId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Användare>()
                .HasIndex(u => u.Användarnamn)
                .IsUnique();
        }
    }
}