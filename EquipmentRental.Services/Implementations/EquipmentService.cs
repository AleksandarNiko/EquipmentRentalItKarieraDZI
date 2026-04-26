using EquipmentRental.Data;
using EquipmentRental.Data.Models;
using EquipmentRental.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EquipmentRental.Services.Implementations
{
    /// <summary>
    /// Business logic service for managing equipment items.
    /// Operates over AppDbContext (data access layer).
    /// </summary>
    public class EquipmentService : IEquipmentService
    {
        private readonly AppDbContext db;

        public EquipmentService(AppDbContext db)
        {
            this.db = db;
        }

        /// <summary>Returns all equipment items ordered by name.</summary>
        public async Task<IEnumerable<EquipmentItem>> GetAllAsync()
        {
            return await db.EquipmentItems
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        /// <summary>Returns equipment items whose name contains the search term (case-insensitive).</summary>
        public async Task<IEnumerable<EquipmentItem>> SearchByNameAsync(string name)
        {
            return await db.EquipmentItems
                .Where(e => e.Name.Contains(name))
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<EquipmentItem?> GetByIdAsync(int id)
        {
            return await db.EquipmentItems.FindAsync(id);
        }

        /// <summary>Adds a new equipment item to the database.</summary>
        public async Task CreateAsync(EquipmentItem item)
        {
            db.EquipmentItems.Add(item);
            await db.SaveChangesAsync();
        }

        /// <summary>Updates an existing equipment item.</summary>
        public async Task UpdateAsync(EquipmentItem item)
        {
            db.EquipmentItems.Update(item);
            await db.SaveChangesAsync();
        }

        /// <summary>Deletes the equipment item with the given ID.</summary>
        public async Task DeleteAsync(int id)
        {
            var item = await db.EquipmentItems.FindAsync(id);
            if (item != null)
            {
                db.EquipmentItems.Remove(item);
                await db.SaveChangesAsync();
            }
        }
    }
}
