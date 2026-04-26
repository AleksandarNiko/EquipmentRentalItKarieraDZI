using EquipmentRental.Data.Models;
using EquipmentRental.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EquipmentRental.Web.Controllers
{
    /// <summary>
    /// Administrator-only controller for managing regular user accounts.
    /// Admins cannot create other administrators through this controller.
    /// </summary>
    [Authorize(Roles = "Administrator")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<UsersController> logger;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            ILogger<UsersController> logger)
        {
            this.userManager = userManager;
            this.logger      = logger;
        }

        // ── List users ──────────────────────────────────────────────────────────

        /// <summary>Shows all users with their roles.</summary>
        public async Task<IActionResult> Index()
        {
            var allUsers = await userManager.Users.ToListAsync();

            var viewModels = new List<UserListItemViewModel>();
            foreach (var u in allUsers)
            {
                var roles = await userManager.GetRolesAsync(u);
                viewModels.Add(new UserListItemViewModel
                {
                    Id        = u.Id,
                    UserName  = u.UserName ?? string.Empty,
                    Email     = u.Email,
                    FirstName = u.FirstName,
                    LastName  = u.LastName,
                    Role      = roles.FirstOrDefault() ?? "User"
                });
            }

            return View(viewModels);
        }

        // ── Create user ─────────────────────────────────────────────────────────

        public IActionResult Create() => View(new CreateUserViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = new ApplicationUser
            {
                UserName       = vm.UserName,
                Email          = vm.Email,
                EmailConfirmed = true,
                FirstName      = vm.FirstName,
                LastName       = vm.LastName
            };

            var result = await userManager.CreateAsync(user, vm.Password);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(vm);
            }

            // Regular users always get the "User" role; admins cannot be created via UI
            await userManager.AddToRoleAsync(user, "User");

            logger.LogInformation("Admin created user: {UserName}", user.UserName);
            TempData["Success"] = "Потребителят беше създаден успешно.";
            return RedirectToAction(nameof(Index));
        }

        // ── Edit user ───────────────────────────────────────────────────────────

        public async Task<IActionResult> Edit(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var vm = new EditUserViewModel
            {
                Id        = user.Id,
                UserName  = user.UserName ?? string.Empty,
                Email     = user.Email    ?? string.Empty,
                FirstName = user.FirstName,
                LastName  = user.LastName
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await userManager.FindByIdAsync(vm.Id);
            if (user == null) return NotFound();

            user.UserName  = vm.UserName;
            user.Email     = vm.Email;
            user.FirstName = vm.FirstName;
            user.LastName  = vm.LastName;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(vm);
            }

            TempData["Success"] = "Потребителят беше обновен.";
            return RedirectToAction(nameof(Index));
        }

        // ── Delete user ─────────────────────────────────────────────────────────

        public async Task<IActionResult> Delete(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await userManager.GetRolesAsync(user);
            var vm = new UserListItemViewModel
            {
                Id        = user.Id,
                UserName  = user.UserName ?? string.Empty,
                Email     = user.Email,
                FirstName = user.FirstName,
                LastName  = user.LastName,
                Role      = roles.FirstOrDefault() ?? "User"
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Prevent deleting the built-in admin account
            if (user.Email == "admin@local")
            {
                TempData["Error"] = "Не можете да изтриете системния администраторски акаунт.";
                return RedirectToAction(nameof(Index));
            }

            await userManager.DeleteAsync(user);
            logger.LogInformation("Admin deleted user: {Id}", id);
            TempData["Success"] = "Потребителят беше изтрит.";
            return RedirectToAction(nameof(Index));
        }
    }
}
