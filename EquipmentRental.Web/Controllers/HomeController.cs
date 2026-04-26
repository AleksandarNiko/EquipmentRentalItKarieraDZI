using EquipmentRental.Data.Models;
using EquipmentRental.Services.Interfaces;
using EquipmentRental.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EquipmentRental.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEquipmentService equipmentService;
        private readonly IRentalRequestService rentalService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<HomeController> logger;

        public HomeController(
            IEquipmentService equipmentService,
            IRentalRequestService rentalService,
            UserManager<ApplicationUser> userManager,
            ILogger<HomeController> logger)
        {
            this.equipmentService = equipmentService;
            this.rentalService    = rentalService;
            this.userManager      = userManager;
            this.logger           = logger;
        }

        /// <summary>
        /// Landing page. Admins see dashboard statistics; regular users see the equipment list.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Administrator"))
            {
                // Admin dashboard: aggregate counts
                var allUsers = userManager.Users.Count();
                var vm = new AdminDashboardViewModel
                {
                    UserCount      = allUsers,
                    EquipmentCount = (await equipmentService.GetAllAsync()).Count(),
                    RequestCount   = await rentalService.CountAllAsync(),
                    PendingCount   = await rentalService.CountPendingAsync()
                };
                return View("AdminDashboard", vm);
            }

            // Regular users see the equipment catalogue
            var equipment = await equipmentService.GetAllAsync();
            return View("UserHome", equipment);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
