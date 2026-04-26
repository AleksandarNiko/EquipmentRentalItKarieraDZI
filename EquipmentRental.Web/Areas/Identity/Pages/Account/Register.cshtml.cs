
using EquipmentRental.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EquipmentRental.Web.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger<RegisterModel> logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger)
        {
            this.userManager   = userManager;
            this.signInManager = signInManager;
            this.logger        = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Собственото име е задължително.")]
            [MaxLength(100)]
            [Display(Name = "Собствено име")]
            public string FirstName { get; set; } = null!;

            [Required(ErrorMessage = "Фамилията е задължителна.")]
            [MaxLength(100)]
            [Display(Name = "Фамилия")]
            public string LastName { get; set; } = null!;

            [Required(ErrorMessage = "Потребителското име е задължително.")]
            [MaxLength(256)]
            [Display(Name = "Потребителско име")]
            public string UserName { get; set; } = null!;

            [Required(ErrorMessage = "Имейлът е задължителен.")]
            [EmailAddress(ErrorMessage = "Невалиден имейл адрес.")]
            [Display(Name = "Имейл")]
            public string Email { get; set; } = null!;

            [Required(ErrorMessage = "Паролата е задължителна.")]
            [MinLength(6, ErrorMessage = "Паролата трябва да е поне 6 символа.")]
            [DataType(DataType.Password)]
            [Display(Name = "Парола")]
            public string Password { get; set; } = null!;

            [DataType(DataType.Password)]
            [Display(Name = "Потвърди парола")]
            [Compare("Password", ErrorMessage = "Паролите не съвпадат.")]
            public string ConfirmPassword { get; set; } = null!;
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            var user = new ApplicationUser
            {
                UserName       = Input.UserName,
                Email          = Input.Email,
                EmailConfirmed = true,  
                FirstName      = Input.FirstName,
                LastName       = Input.LastName
            };

            var result = await userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "User");
                logger.LogInformation("New user registered: {UserName}", user.UserName);

                await signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }
    }
}
