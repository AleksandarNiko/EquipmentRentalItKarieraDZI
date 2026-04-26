using EquipmentRental.Data.Models;
using EquipmentRental.Services.Implementations;
using FluentAssertions;
using Xunit;

namespace EquipmentRental.Tests;

/// <summary>
/// Unit tests for <see cref="RentalRequestService"/>.
/// Uses the EF Core in-memory provider so no real database is required.
/// </summary>
public class RentalRequestServiceTests
{
    // ── Helpers ────────────────────────────────────────────────────────────────

    /// <summary>Seeds an equipment item and returns its auto-generated ID.</summary>
    private static async Task<int> SeedEquipmentAsync(EquipmentRental.Data.AppDbContext db, string name = "Ladder")
    {
        var item = new EquipmentItem { Name = name, AvailableQuantity = 10, Condition = "New" };
        db.EquipmentItems.Add(item);
        await db.SaveChangesAsync();
        return item.Id;
    }

    private static RentalRequest BuildRequest(string userId = "user-1") => new()
    {
        UserId    = userId,
        UserName  = "testuser",
        StartDate = DateTime.Today,
        EndDate   = DateTime.Today.AddDays(3)
    };

    // ── CreateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_SetsPendingStatusAndTimestamp()
    {
        using var db = DbContextFactory.Create();
        var equipId = await SeedEquipmentAsync(db);
        var svc     = new RentalRequestService(db);

        var request = BuildRequest();
        var before  = DateTime.UtcNow;

        await svc.CreateAsync(request, new Dictionary<int, int> { { equipId, 2 } });

        var saved = db.RentalRequests.Single();
        saved.Status.Should().Be(RentalStatus.Pending);
        saved.CreatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsRentalRequestItems()
    {
        using var db = DbContextFactory.Create();
        var equipId1 = await SeedEquipmentAsync(db, "Ladder");
        var equipId2 = await SeedEquipmentAsync(db, "Scaffold");
        var svc      = new RentalRequestService(db);

        await svc.CreateAsync(BuildRequest(), new Dictionary<int, int>
        {
            { equipId1, 1 },
            { equipId2, 3 }
        });

        db.RentalRequestItems.Should().HaveCount(2);
        db.RentalRequestItems.Should().Contain(ri => ri.EquipmentItemId == equipId1 && ri.Quantity == 1);
        db.RentalRequestItems.Should().Contain(ri => ri.EquipmentItemId == equipId2 && ri.Quantity == 3);
    }

    [Fact]
    public async Task CreateAsync_ZeroQuantityItems_AreNotPersisted()
    {
        using var db = DbContextFactory.Create();
        var equipId = await SeedEquipmentAsync(db);
        var svc     = new RentalRequestService(db);

        // A quantity of 0 should be filtered out
        await svc.CreateAsync(BuildRequest(), new Dictionary<int, int> { { equipId, 0 } });

        db.RentalRequestItems.Should().BeEmpty();
    }

    // ── GetAllAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllRequests_OrderedByCreatedAtDescending()
    {
        using var db = DbContextFactory.Create();
        var svc = new RentalRequestService(db);

        // Add with explicit timestamps to test ordering
        db.RentalRequests.AddRange(
            new RentalRequest
            {
                UserId = "u1", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1),
                Status = RentalStatus.Pending, CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new RentalRequest
            {
                UserId = "u2", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1),
                Status = RentalStatus.Approved, CreatedAt = DateTime.UtcNow
            });
        await db.SaveChangesAsync();

        var result = (await svc.GetAllAsync()).ToList();

        result.Should().HaveCount(2);
        result[0].CreatedAt.Should().BeAfter(result[1].CreatedAt);
    }

    // ── GetByUserAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByUserAsync_ReturnsOnlyRequestsForThatUser()
    {
        using var db = DbContextFactory.Create();
        db.RentalRequests.AddRange(
            new RentalRequest { UserId = "user-A", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1), Status = RentalStatus.Pending, CreatedAt = DateTime.UtcNow },
            new RentalRequest { UserId = "user-A", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2), Status = RentalStatus.Approved, CreatedAt = DateTime.UtcNow },
            new RentalRequest { UserId = "user-B", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1), Status = RentalStatus.Pending, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var svc    = new RentalRequestService(db);
        var result = (await svc.GetByUserAsync("user-A")).ToList();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.UserId == "user-A");
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsRequestWithItems()
    {
        using var db = DbContextFactory.Create();
        var equipId = await SeedEquipmentAsync(db);
        var svc     = new RentalRequestService(db);

        var request = BuildRequest();
        await svc.CreateAsync(request, new Dictionary<int, int> { { equipId, 2 } });

        var result = await svc.GetByIdAsync(request.Id);

        result.Should().NotBeNull();
        result!.RentalRequestItems.Should().HaveCount(1);
        result.RentalRequestItems.First().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        using var db = DbContextFactory.Create();
        var svc = new RentalRequestService(db);

        var result = await svc.GetByIdAsync(999);

        result.Should().BeNull();
    }

    // ── UpdateStatusAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatusAsync_ExistingRequest_ChangesStatus()
    {
        using var db = DbContextFactory.Create();
        var request = new RentalRequest
        {
            UserId    = "u1",
            StartDate = DateTime.Today,
            EndDate   = DateTime.Today.AddDays(1),
            Status    = RentalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        db.RentalRequests.Add(request);
        await db.SaveChangesAsync();

        var svc = new RentalRequestService(db);
        await svc.UpdateStatusAsync(request.Id, RentalStatus.Approved);

        var updated = await db.RentalRequests.FindAsync(request.Id);
        updated!.Status.Should().Be(RentalStatus.Approved);
    }

    [Fact]
    public async Task UpdateStatusAsync_NonExistingId_DoesNotThrow()
    {
        using var db = DbContextFactory.Create();
        var svc = new RentalRequestService(db);

        var act = async () => await svc.UpdateStatusAsync(999, RentalStatus.Approved);

        await act.Should().NotThrowAsync();
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingRequest_RemovesRequestAndItems()
    {
        using var db = DbContextFactory.Create();
        var equipId = await SeedEquipmentAsync(db);
        var svc     = new RentalRequestService(db);

        var request = BuildRequest();
        await svc.CreateAsync(request, new Dictionary<int, int> { { equipId, 1 } });

        db.RentalRequests.Should().HaveCount(1);
        db.RentalRequestItems.Should().HaveCount(1);

        await svc.DeleteAsync(request.Id);

        db.RentalRequests.Should().BeEmpty();
        db.RentalRequestItems.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_DoesNotThrow()
    {
        using var db = DbContextFactory.Create();
        var svc = new RentalRequestService(db);

        var act = async () => await svc.DeleteAsync(999);

        await act.Should().NotThrowAsync();
    }

    // ── CountAllAsync / CountPendingAsync ──────────────────────────────────────

    [Fact]
    public async Task CountAllAsync_ReturnsCorrectTotal()
    {
        using var db = DbContextFactory.Create();
        db.RentalRequests.AddRange(
            new RentalRequest { UserId = "u1", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1), Status = RentalStatus.Pending,  CreatedAt = DateTime.UtcNow },
            new RentalRequest { UserId = "u2", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1), Status = RentalStatus.Approved, CreatedAt = DateTime.UtcNow },
            new RentalRequest { UserId = "u3", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1), Status = RentalStatus.Rejected, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var svc   = new RentalRequestService(db);
        var count = await svc.CountAllAsync();

        count.Should().Be(3);
    }

    [Fact]
    public async Task CountPendingAsync_ReturnsOnlyPendingCount()
    {
        using var db = DbContextFactory.Create();
        db.RentalRequests.AddRange(
            new RentalRequest { UserId = "u1", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1), Status = RentalStatus.Pending,  CreatedAt = DateTime.UtcNow },
            new RentalRequest { UserId = "u2", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1), Status = RentalStatus.Pending,  CreatedAt = DateTime.UtcNow },
            new RentalRequest { UserId = "u3", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1), Status = RentalStatus.Approved, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var svc   = new RentalRequestService(db);
        var count = await svc.CountPendingAsync();

        count.Should().Be(2);
    }
}
