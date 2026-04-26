using EquipmentRental.Data;
using EquipmentRental.Data.Models;
using EquipmentRental.Services.Implementations;
using EquipmentRental.Services.Interfaces;
using EquipmentRental.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Single unified context: holds both domain tables AND Identity tables (Users, Roles, …)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ── ASP.NET Core Identity ─────────────────────────────────────────────────────
// Uses ApplicationUser (which adds FirstName / LastName to the AspNetUsers table)
// and stores everything in the same AppDbContext so Users + Roles tables are visible
// alongside EquipmentItems, RentalRequests, etc.
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultUI();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Administrator"));
});

builder.Services.AddScoped<IEquipmentService, EquipmentService>();
builder.Services.AddScoped<IRentalRequestService, RentalRequestService>();
builder.Services.AddScoped<RoleInitializer>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try { db.Database.Migrate(); }
    catch { db.Database.EnsureCreated(); }

    // Seed roles and the default admin account
    var initializer = scope.ServiceProvider.GetRequiredService<RoleInitializer>();
    try { await initializer.InitializeAsync(); }
    catch (Exception ex) { logger.LogError(ex, "Error during role initialisation."); }
}

if (app.Environment.IsDevelopment())
    app.UseMigrationsEndPoint();
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
