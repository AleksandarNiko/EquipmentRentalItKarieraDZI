using EquipmentRental.Data.Models;
using EquipmentRental.Services.Implementations;
using FluentAssertions;
using Xunit;

namespace EquipmentRental.Tests;

/// <summary>
/// Unit tests for <see cref="EquipmentService"/>.
/// Each test creates its own in-memory database to ensure complete isolation.
/// </summary>
public class EquipmentServiceTests
{
    // ── GetAllAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllItems_OrderedByName()
    {
        // Arrange
        using var db = DbContextFactory.Create();
        db.EquipmentItems.AddRange(
            new EquipmentItem { Name = "Zebra Drill",  AvailableQuantity = 2, Condition = "New" },
            new EquipmentItem { Name = "Alpha Hammer", AvailableQuantity = 5, Condition = "New" },
            new EquipmentItem { Name = "Beta Saw",     AvailableQuantity = 3, Condition = "Used" });
        await db.SaveChangesAsync();

        var svc = new EquipmentService(db);

        // Act
        var result = (await svc.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Alpha Hammer");
        result[1].Name.Should().Be("Beta Saw");
        result[2].Name.Should().Be("Zebra Drill");
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        using var db = DbContextFactory.Create();
        var svc = new EquipmentService(db);

        var result = await svc.GetAllAsync();

        result.Should().BeEmpty();
    }

    // ── SearchByNameAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task SearchByNameAsync_MatchingTerm_ReturnsFilteredItems()
    {
        using var db = DbContextFactory.Create();
        db.EquipmentItems.AddRange(
            new EquipmentItem { Name = "Electric Drill", AvailableQuantity = 1, Condition = "New" },
            new EquipmentItem { Name = "Cordless Drill",  AvailableQuantity = 2, Condition = "New" },
            new EquipmentItem { Name = "Circular Saw",    AvailableQuantity = 1, Condition = "New" });
        await db.SaveChangesAsync();

        var svc = new EquipmentService(db);

        var result = (await svc.SearchByNameAsync("Drill")).ToList();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.Name.Contains("Drill"));
    }

    [Fact]
    public async Task SearchByNameAsync_NoMatch_ReturnsEmptyList()
    {
        using var db = DbContextFactory.Create();
        db.EquipmentItems.Add(new EquipmentItem { Name = "Hammer", AvailableQuantity = 1, Condition = "New" });
        await db.SaveChangesAsync();

        var svc = new EquipmentService(db);

        var result = await svc.SearchByNameAsync("Drill");

        result.Should().BeEmpty();
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCorrectItem()
    {
        using var db = DbContextFactory.Create();
        var item = new EquipmentItem { Name = "Wrench", AvailableQuantity = 4, Condition = "Used" };
        db.EquipmentItems.Add(item);
        await db.SaveChangesAsync();

        var svc = new EquipmentService(db);

        var result = await svc.GetByIdAsync(item.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Wrench");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        using var db = DbContextFactory.Create();
        var svc = new EquipmentService(db);

        var result = await svc.GetByIdAsync(999);

        result.Should().BeNull();
    }

    // ── CreateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidItem_PersistsToDatabase()
    {
        using var db = DbContextFactory.Create();
        var svc = new EquipmentService(db);

        var item = new EquipmentItem
        {
            Name              = "Nail Gun",
            Description       = "Pneumatic nail gun",
            AvailableQuantity = 3,
            Condition         = "New"
        };

        await svc.CreateAsync(item);

        db.EquipmentItems.Should().ContainSingle(e => e.Name == "Nail Gun");
        db.EquipmentItems.Single().AvailableQuantity.Should().Be(3);
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingItem_SavesChanges()
    {
        using var db = DbContextFactory.Create();
        var item = new EquipmentItem { Name = "Old Name", AvailableQuantity = 1, Condition = "New" };
        db.EquipmentItems.Add(item);
        await db.SaveChangesAsync();

        var svc = new EquipmentService(db);
        item.Name              = "New Name";
        item.AvailableQuantity = 10;

        await svc.UpdateAsync(item);

        var updated = await db.EquipmentItems.FindAsync(item.Id);
        updated!.Name.Should().Be("New Name");
        updated.AvailableQuantity.Should().Be(10);
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingItem_RemovesFromDatabase()
    {
        using var db = DbContextFactory.Create();
        var item = new EquipmentItem { Name = "Chisel", AvailableQuantity = 2, Condition = "Used" };
        db.EquipmentItems.Add(item);
        await db.SaveChangesAsync();

        var svc = new EquipmentService(db);

        await svc.DeleteAsync(item.Id);

        db.EquipmentItems.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_DoesNotThrow()
    {
        using var db = DbContextFactory.Create();
        var svc = new EquipmentService(db);

        // Should silently do nothing when the ID is not found
        var act = async () => await svc.DeleteAsync(999);

        await act.Should().NotThrowAsync();
    }
}
