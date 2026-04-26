using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquipmentRental.Data.Models
{
    /// <summary>
    /// Join table implementing the many-to-many relationship between
    /// RentalRequest and EquipmentItem, also storing the requested quantity.
    /// </summary>
    public class RentalRequestItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RentalRequestId { get; set; }

        [Required]
        public int EquipmentItemId { get; set; }

        /// <summary>Quantity of this equipment item requested in the rental.</summary>
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        // Navigation properties
        [ForeignKey(nameof(RentalRequestId))]
        public RentalRequest? RentalRequest { get; set; }

        [ForeignKey(nameof(EquipmentItemId))]
        public EquipmentItem? EquipmentItem { get; set; }
    }
}
