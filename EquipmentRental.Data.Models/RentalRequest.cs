using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EquipmentRental.Data.Models
{
    /// <summary>
    /// Represents a rental request submitted by a user.
    /// </summary>
    public class RentalRequest
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Start date of the rental period.</summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>End date of the rental period.</summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>Optional comment or purpose for the rental.</summary>
        [MaxLength(500)]
        public string? Comment { get; set; }

        /// <summary>Current status: Pending, Approved, or Rejected.</summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = RentalStatus.Pending;

        /// <summary>Identity user ID of the creator.</summary>
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = null!;

        /// <summary>Display name (username/email) of the user who created this request.</summary>
        [MaxLength(256)]
        public string? UserName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation: many-to-many with EquipmentItem via RentalRequestItem
        public ICollection<RentalRequestItem> RentalRequestItems { get; set; } = new List<RentalRequestItem>();
    }

    /// <summary>Allowed status values for RentalRequest.</summary>
    public static class RentalStatus
    {
        public const string Pending  = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
    }
}
