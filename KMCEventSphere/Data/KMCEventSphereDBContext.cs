using Microsoft.EntityFrameworkCore;
using KMCEventSphere.Models;

namespace KMCEventSphere.Data
{
    public class KMCEventSphereDBContext : DbContext
    {
        public KMCEventSphereDBContext(DbContextOptions<KMCEventSphereDBContext> options) : base(options)
        { }

        public DbSet<Event> Events { get; set; }
        public DbSet<Organizer> Organizers { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>()
                .Property(e => e.Price)
                .HasColumnType("decimal(18,2)");

            // UNIQUE constraints for Users
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Organizer <-> User 1:1 relationship (CASCADE DELETE)
            modelBuilder.Entity<Organizer>()
                .HasOne(o => o.User)
                .WithOne(u => u.Organizer)
                .HasForeignKey<Organizer>(o => o.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Organizer additional fields
            modelBuilder.Entity<Organizer>()
                .Property(o => o.Phone)
                .HasMaxLength(20);

            modelBuilder.Entity<Organizer>()
                .Property(o => o.OrganizationName)
                .HasMaxLength(200);

            modelBuilder.Entity<Organizer>()
                .Property(o => o.Address)
                .HasMaxLength(500);

            modelBuilder.Entity<Organizer>()
                .Property(o => o.Description)
                .HasMaxLength(1000);

            modelBuilder.Entity<Registration>();

            modelBuilder.Entity<Event>()
              .HasOne(e => e.Organizer)
              .WithMany(o => o.Events)
              .HasForeignKey(e => e.OrganizerID)
              .OnDelete(DeleteBehavior.SetNull);
        }
    }
}