using EquipmentRental.Data;
using Microsoft.EntityFrameworkCore;

namespace EquipmentRental.Tests;

/// <summary>
/// Provides a fresh in-memory <see cref="AppDbContext"/> for each test,
/// avoiding any cross-test state pollution.
/// The in-memory provider satisfies both the domain tables and the
/// Identity tables that AppDbContext now owns.
/// </summary>
public static class DbContextFactory
{
    /// <summary>
    /// Creates a new <see cref="AppDbContext"/> backed by an in-memory database
    /// with a unique name so tests never share state.
    /// </summary>
    public static AppDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
