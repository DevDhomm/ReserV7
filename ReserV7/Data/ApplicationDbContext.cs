using Microsoft.EntityFrameworkCore;
using ReserV7.Models;

namespace ReserV7.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Salle> Salles => Set<Salle>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<Creneau> Creneaux => Set<Creneau>();
        public DbSet<Equipement> Equipements => Set<Equipement>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Reservation relationships
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Salle)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.SalleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Creneau)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.CreneauId)
                .OnDelete(DeleteBehavior.SetNull);

            // Equipement relationships
            modelBuilder.Entity<Equipement>()
                .HasOne(e => e.Salle)
                .WithMany(s => s.Equipements)
                .HasForeignKey(e => e.SalleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Salle configuration
            modelBuilder.Entity<Salle>()
                .HasIndex(s => s.Nom)
                .IsUnique();

            // Note: All seed data is now handled by the initialize_database.sql script
            // This DbContext only defines the relationships and indices
        }
    }
}
