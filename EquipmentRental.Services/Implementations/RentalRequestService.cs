using EquipmentRental.Data;
using EquipmentRental.Data.Models;
using EquipmentRental.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EquipmentRental.Services.Implementations
{
    /// <summary>
    /// Business logic service for managing rental requests.
    /// Handles creation, status updates, and retrieval.
    /// </summary>
    public class RentalRequestService : IRentalRequestService
    {
        private readonly AppDbContext db;

        public RentalRequestService(AppDbContext db)
        {
            this.db = db;
        }

        /// <summary>Returns all rental requests including associated items and equipment details.</summary>
        public async Task<IEnumerable<RentalRequest>> GetAllAsync()
        {
            return await db.RentalRequests
                .Include(r => r.RentalRequestItems)
                    .ThenInclude(ri => ri.EquipmentItem)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        /// <summary>Returns all requests submitted by a specific user.</summary>
        public async Task<IEnumerable<RentalRequest>> GetByUserAsync(string userId)
        {
            return await db.RentalRequests
                .Include(r => r.RentalRequestItems)
                    .ThenInclude(ri => ri.EquipmentItem)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<RentalRequest?> GetByIdAsync(int id)
        {
            return await db.RentalRequests
                .Include(r => r.RentalRequestItems)
                    .ThenInclude(ri => ri.EquipmentItem)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// Creates a new rental request with status 'Pending' and attaches the selected
        /// equipment items with their requested quantities.
        /// </summary>
        public async Task CreateAsync(RentalRequest request, Dictionary<int, int> equipmentQuantities)
        {
            // New requests always start as Pending
            request.Status = RentalStatus.Pending;
            request.CreatedAt = DateTime.UtcNow;

            // Build join-table entries for each selected equipment item
            request.RentalRequestItems = equipmentQuantities
                .Where(kv => kv.Value > 0)
                .Select(kv => new RentalRequestItem
                {
                    EquipmentItemId = kv.Key,
                    Quantity = kv.Value
                }).ToList();

            db.RentalRequests.Add(request);
            await db.SaveChangesAsync();
        }

        /// <summary>Updates the status of an existing rental request.</summary>
        public async Task UpdateStatusAsync(int id, string newStatus)
        {
            var request = await db.RentalRequests.FindAsync(id);
            if (request != null)
            {
                request.Status = newStatus;
                await db.SaveChangesAsync();
            }
        }

        /// <summary>Deletes a rental request (only allowed when status is Pending).</summary>
        public async Task DeleteAsync(int id)
        {
            var request = await db.RentalRequests
                .Include(r => r.RentalRequestItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request != null)
            {
                db.RentalRequestItems.RemoveRange(request.RentalRequestItems);
                db.RentalRequests.Remove(request);
                await db.SaveChangesAsync();
            }
        }

        public async Task<int> CountAllAsync() =>
            await db.RentalRequests.CountAsync();

        public async Task<int> CountPendingAsync() =>
            await db.RentalRequests.CountAsync(r => r.Status == RentalStatus.Pending);
    }
}
