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
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships for Beställning and Beställningsdetaljer
            modelBuilder.Entity<Beställning>()
                .HasMany(b => b.Beställningsdetaljer)
                .WithOne()
                .HasForeignKey(bd => bd.BeställningId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship for Beställning and Butik
            modelBuilder.Entity<Beställning>()
                .HasOne(b => b.Butik)
                .WithMany()
                .HasForeignKey(b => b.ButikId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationship for Beställningsdetalj and Produkt
            modelBuilder.Entity<Beställningsdetalj>()
                .HasOne(bd => bd.Produkt)
                .WithMany()
                .HasForeignKey(bd => bd.ProduktId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure Användarnamn is unique
            modelBuilder.Entity<Användare>()
                .HasIndex(u => u.Användarnamn)
                .IsUnique();

            // Configure PasswordResetToken constraints
            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(prt => prt.Username);

            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(prt => prt.Token)
                .IsUnique();

            // Configure soft delete filter for Produkter
            modelBuilder.Entity<Produkt>()
                .HasQueryFilter(p => !p.IsDeleted);

            // Log that the model is being configured
            Console.WriteLine("Configuring database model with cascading deletes for Beställningsdetaljer.");
        }
    }
}