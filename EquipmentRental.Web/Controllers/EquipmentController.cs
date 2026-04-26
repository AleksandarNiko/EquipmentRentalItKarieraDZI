using EquipmentRental.Data.Models;
using EquipmentRental.Services.Interfaces;
using EquipmentRental.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentRental.Web.Controllers
{
    /// <summary>
    /// Handles equipment management (admin: full CRUD; users: browse and search).
    /// </summary>
    public class EquipmentController : Controller
    {
        private readonly IEquipmentService equipmentService;
        private readonly ILogger<EquipmentController> logger;

        public EquipmentController(IEquipmentService equipmentService, ILogger<EquipmentController> logger)
        {
            this.equipmentService = equipmentService;
            this.logger = logger;
        }

        // ── Browse / Search (all authenticated users) ──────────────────────────

        /// <summary>Lists all equipment or filters by name when a search term is provided.</summary>
        [Authorize]
        public async Task<IActionResult> Index(string? search)
        {
            ViewBag.Search = search;

            var items = string.IsNullOrWhiteSpace(search)
                ? await equipmentService.GetAllAsync()
                : await equipmentService.SearchByNameAsync(search);

            return View(items);
        }

        // ── Admin: Create ───────────────────────────────────────────────────────

        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View(new EquipmentFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(EquipmentFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var item = new EquipmentItem
            {
                Name              = vm.Name,
                Description       = vm.Description,
                AvailableQuantity = vm.AvailableQuantity,
                ImageUrl          = vm.ImageUrl,
                Condition         = vm.Condition
            };

            await equipmentService.CreateAsync(item);
            logger.LogInformation("Equipment created: {Name}", item.Name);

            TempData["Success"] = "Оборудването беше добавено успешно.";
            return RedirectToAction(nameof(Index));
        }

        // ── Admin: Edit ─────────────────────────────────────────────────────────

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await equipmentService.GetByIdAsync(id);
            if (item == null) return NotFound();

            var vm = new EquipmentFormViewModel
            {
                Id                = item.Id,
                Name              = item.Name,
                Description       = item.Description,
                AvailableQuantity = item.AvailableQuantity,
                ImageUrl          = item.ImageUrl,
                Condition         = item.Condition
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(EquipmentFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var item = await equipmentService.GetByIdAsync(vm.Id);
            if (item == null) return NotFound();

            item.Name              = vm.Name;
            item.Description       = vm.Description;
            item.AvailableQuantity = vm.AvailableQuantity;
            item.ImageUrl          = vm.ImageUrl;
            item.Condition         = vm.Condition;

            await equipmentService.UpdateAsync(item);
            logger.LogInformation("Equipment updated: {Id} {Name}", item.Id, item.Name);

            TempData["Success"] = "Оборудването беше обновено успешно.";
            return RedirectToAction(nameof(Index));
        }

        // ── Admin: Delete ───────────────────────────────────────────────────────

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await equipmentService.GetByIdAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await equipmentService.DeleteAsync(id);
            logger.LogInformation("Equipment deleted: {Id}", id);

            TempData["Success"] = "Оборудването беше изтрито.";
            return RedirectToAction(nameof(Index));
        }
    }
}
