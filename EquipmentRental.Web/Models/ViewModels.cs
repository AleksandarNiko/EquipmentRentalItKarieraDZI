using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EquipmentRental.Web.Models
{
    // ──────────────────────────────────────────────
    // Equipment ViewModels
    // ──────────────────────────────────────────────

    /// <summary>ViewModel for creating or editing an equipment item.</summary>
    public class EquipmentFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Името е задължително.")]
        [MaxLength(64, ErrorMessage = "Името може да е най-много 64 символа.")]
        [Display(Name = "Име")]
        public string Name { get; set; } = null!;

        [MaxLength(255, ErrorMessage = "Описанието може да е най-много 255 символа.")]
        [Display(Name = "Кратко описание")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Количеството е задължително.")]
        [Range(0, int.MaxValue, ErrorMessage = "Количеството трябва да е неотрицателно.")]
        [Display(Name = "Налично количество")]
        public int AvailableQuantity { get; set; }

        [Display(Name = "Снимка (URL)")]
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Моля, изберете състояние.")]
        [Display(Name = "Състояние")]
        public string Condition { get; set; } = "New";
    }

    // ──────────────────────────────────────────────
    // Rental Request ViewModels
    // ──────────────────────────────────────────────

    /// <summary>A single equipment line selected for a rental request.</summary>
    public class RentalItemLineViewModel
    {
        public int EquipmentItemId { get; set; }
        public string EquipmentName { get; set; } = null!;
        public bool IsSelected { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }

    /// <summary>ViewModel used when creating a new rental request.</summary>
    public class CreateRentalRequestViewModel
    {
        [Required(ErrorMessage = "Началната дата е задължителна.")]
        [DataType(DataType.Date)]
        [Display(Name = "Начална дата")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Крайната дата е задължителна.")]
        [DataType(DataType.Date)]
        [Display(Name = "Крайна дата")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

        [MaxLength(500)]
        [Display(Name = "Коментар / цел на използване")]
        public string? Comment { get; set; }

        /// <summary>List of all available equipment items (populated from DB).</summary>
        public List<RentalItemLineViewModel> Items { get; set; } = new();
    }

    // ──────────────────────────────────────────────
    // Admin Dashboard ViewModel
    // ──────────────────────────────────────────────

    /// <summary>Summary statistics shown on the admin home page.</summary>
    public class AdminDashboardViewModel
    {
        public int UserCount { get; set; }
        public int EquipmentCount { get; set; }
        public int RequestCount { get; set; }
        public int PendingCount { get; set; }
    }

    // ──────────────────────────────────────────────
    // User management ViewModels
    // ──────────────────────────────────────────────

    /// <summary>Displays a user row in the admin user list.</summary>
    public class UserListItemViewModel
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Role { get; set; } = null!;
    }

    /// <summary>Form for creating a new regular user by an admin.</summary>
    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "Потребителско име")]
        public string UserName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [Display(Name = "Имейл")]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        [Display(Name = "Парола")]
        public string Password { get; set; } = null!;

        [Display(Name = "Собствено име")]
        public string? FirstName { get; set; }

        [Display(Name = "Фамилия")]
        public string? LastName { get; set; }
    }

    /// <summary>Form for editing an existing user by an admin.</summary>
    public class EditUserViewModel
    {
        public string Id { get; set; } = null!;

        [Required]
        [Display(Name = "Потребителско име")]
        public string UserName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [Display(Name = "Имейл")]
        public string Email { get; set; } = null!;

        [Display(Name = "Собствено име")]
        public string? FirstName { get; set; }

        [Display(Name = "Фамилия")]
        public string? LastName { get; set; }
    }

    // ──────────────────────────────────────────────
    // Misc
    // ──────────────────────────────────────────────
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
