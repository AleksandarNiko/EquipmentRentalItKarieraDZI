using EquipmentRental.Data.Models;
using EquipmentRental.Services.Interfaces;
using EquipmentRental.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentRental.Web.Controllers
{
    /// <summary>
    /// Handles rental requests: creation and "my requests" for regular users;
    /// full list and status management for administrators.
    /// </summary>
    [Authorize]
    public class RentalRequestsController : Controller
    {
        private readonly IRentalRequestService rentalService;
        private readonly IEquipmentService equipmentService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<RentalRequestsController> logger;

        public RentalRequestsController(
            IRentalRequestService rentalService,
            IEquipmentService equipmentService,
            UserManager<ApplicationUser> userManager,
            ILogger<RentalRequestsController> logger)
        {
            this.rentalService    = rentalService;
            this.equipmentService = equipmentService;
            this.userManager      = userManager;
            this.logger           = logger;
        }

        // ── User: Create request ────────────────────────────────────────────────

        /// <summary>Shows the form for creating a new rental request with equipment selection.</summary>
        public async Task<IActionResult> Create()
        {
            var equipment = await equipmentService.GetAllAsync();

            var vm = new CreateRentalRequestViewModel
            {
                StartDate = DateTime.Today,
                EndDate   = DateTime.Today.AddDays(1),
                Items     = equipment.Select(e => new RentalItemLineViewModel
                {
                    EquipmentItemId = e.Id,
                    EquipmentName   = e.Name,
                    IsSelected      = false,
                    Quantity        = 1
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRentalRequestViewModel vm)
        {
            // Validate at least one item is selected with quantity > 0
            var selected = vm.Items.Where(i => i.IsSelected && i.Quantity > 0).ToList();
            if (!selected.Any())
            {
                ModelState.AddModelError(string.Empty,
                    "Моля, изберете поне един артикул с количество по-голямо от 0.");
            }

            if (vm.EndDate <= vm.StartDate)
            {
                ModelState.AddModelError(nameof(vm.EndDate),
                    "Крайната дата трябва да е след началната дата.");
            }

            if (!ModelState.IsValid)
            {
                // Re-populate equipment names in case validation fails
                var equipment = await equipmentService.GetAllAsync();
                var equipList = equipment.ToList();
                foreach (var item in vm.Items)
                {
                    var eq = equipList.FirstOrDefault(e => e.Id == item.EquipmentItemId);
                    if (eq != null) item.EquipmentName = eq.Name;
                }
                return View(vm);
            }

            var user = await userManager.GetUserAsync(User);

            var request = new RentalRequest
            {
                StartDate = vm.StartDate,
                EndDate   = vm.EndDate,
                Comment   = vm.Comment,
                UserId    = user!.Id,
                UserName  = user.UserName ?? user.Email
            };

            var quantities = selected.ToDictionary(i => i.EquipmentItemId, i => i.Quantity);
            await rentalService.CreateAsync(request, quantities);

            logger.LogInformation("Rental request created by {User}", user.UserName);
            TempData["Success"] = "Заявката беше подадена успешно.";
            return RedirectToAction(nameof(MyRequests));
        }

        // ── User: My Requests ───────────────────────────────────────────────────

        /// <summary>Shows all rental requests submitted by the currently logged-in user.</summary>
        public async Task<IActionResult> MyRequests()
        {
            var user = await userManager.GetUserAsync(User);
            var requests = await rentalService.GetByUserAsync(user!.Id);
            return View(requests);
        }

        // ── User: Delete own pending request ────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await rentalService.GetByIdAsync(id);
            if (request == null) return NotFound();

            var user = await userManager.GetUserAsync(User);

            // Users may only delete their own requests, and only when still Pending
            if (request.UserId != user!.Id)
                return Forbid();

            if (request.Status != RentalStatus.Pending)
            {
                TempData["Error"] = "Можете да изтривате само заявки със статус 'В изчакване' ";
                return RedirectToAction(nameof(MyRequests));
            }

            await rentalService.DeleteAsync(id);
            TempData["Success"] = "Заявката беше изтрита.";
            return RedirectToAction(nameof(MyRequests));
        }

        // ── Admin: All requests ─────────────────────────────────────────────────

        /// <summary>Administrator view: lists all rental requests with status management.</summary>
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AdminIndex()
        {
            var requests = await rentalService.GetAllAsync();
            return View(requests);
        }

        // ── Admin: Change status ────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            // Only allow the three defined statuses
            var allowed = new[] { RentalStatus.Pending, RentalStatus.Approved, RentalStatus.Rejected };
            if (!allowed.Contains(status))
            {
                TempData["Error"] = "Невалиден статус.";
                return RedirectToAction(nameof(AdminIndex));
            }

            await rentalService.UpdateStatusAsync(id, status);
            logger.LogInformation("Request {Id} status changed to {Status}", id, status);
            TempData["Success"] = "Статусът на заявката беше обновен.";
            return RedirectToAction(nameof(AdminIndex));
        }
    }
}
