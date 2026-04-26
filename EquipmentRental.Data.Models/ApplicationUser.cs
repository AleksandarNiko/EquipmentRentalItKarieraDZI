using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EquipmentRental.Data.Models
{
    /// <summary>
    /// Custom Identity user that extends <see cref="IdentityUser"/> with
    /// domain-specific profile fields (first name, last name).
    /// Stored in the AspNetUsers table together with all standard Identity columns.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>User's first (given) name.</summary>
        [MaxLength(100)]
        public string? FirstName { get; set; }

        /// <summary>User's last (family) name.</summary>
        [MaxLength(100)]
        public string? LastName { get; set; }

        /// <summary>Full display name derived from first + last name, falling back to username.</summary>
        public string DisplayName =>
            string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
                ? UserName ?? Email ?? string.Empty
                : $"{FirstName} {LastName}".Trim();
    }
}
