using EquipmentRental.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EquipmentRental.Data
{
    /// <summary>
    /// Single unified database context for the application.
    /// Inherits from <see cref="IdentityDbContext{TUser}"/> so that
    /// ASP.NET Core Identity tables (AspNetUsers / AspNetRoles, etc.)
    /// are created in the same database alongside the domain tables,
    /// satisfying the requirement for explicit Users and Roles tables.
    /// </summary>
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ── Domain tables ─────────────────────────────────────────────────────

        /// <summary>Equipment items available for rental.</summary>
        public DbSet<EquipmentItem> EquipmentItems { get; set; }

        /// <summary>Rental requests submitted by users.</summary>
        public DbSet<RentalRequest> RentalRequests { get; set; }

        /// <summary>Join table for the many-to-many relation between requests and equipment.</summary>
        public DbSet<RentalRequestItem> RentalRequestItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Must call base so Identity tables (Users, Roles, Claims, etc.) are configured.
            base.OnModelCreating(modelBuilder);

            // ── Many-to-many: RentalRequest <-> EquipmentItem via RentalRequestItem ──

            modelBuilder.Entity<RentalRequestItem>()
                .HasOne(ri => ri.RentalRequest)
                .WithMany(r => r.RentalRequestItems)
                .HasForeignKey(ri => ri.RentalRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RentalRequestItem>()
                .HasOne(ri => ri.EquipmentItem)
                .WithMany(e => e.RentalRequestItems)
                .HasForeignKey(ri => ri.EquipmentItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
