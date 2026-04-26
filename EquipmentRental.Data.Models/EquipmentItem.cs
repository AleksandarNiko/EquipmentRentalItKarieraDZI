using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EquipmentRental.Data.Models
{
    /// <summary>
    /// Represents a piece of equipment available for rental.
    /// </summary>
    public class EquipmentItem
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Name of the equipment, up to 64 characters.</summary>
        [Required]
        [MaxLength(64)]
        public string Name { get; set; } = null!;

        /// <summary>Short description, up to 255 characters.</summary>
        [MaxLength(255)]
        public string? Description { get; set; }

        /// <summary>Available quantity in stock.</summary>
        [Required]
        [Range(0, int.MaxValue)]
        public int AvailableQuantity { get; set; }

        /// <summary>URL or relative path to the equipment image.</summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>Condition of the equipment, e.g. New, Used, NeedsRepair.</summary>
        [Required]
        [MaxLength(50)]
        public string Condition { get; set; } = "New";

        // Navigation: many-to-many with RentalRequest via RentalRequestItem
        public ICollection<RentalRequestItem> RentalRequestItems { get; set; } = new List<RentalRequestItem>();
    }
}
