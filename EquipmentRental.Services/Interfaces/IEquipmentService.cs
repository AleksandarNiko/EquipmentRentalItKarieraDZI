using EquipmentRental.Data.Models;

namespace EquipmentRental.Services.Interfaces
{
    /// <summary>Service interface for managing equipment items.</summary>
    public interface IEquipmentService
    {
        Task<IEnumerable<EquipmentItem>> GetAllAsync();
        Task<IEnumerable<EquipmentItem>> SearchByNameAsync(string name);
        Task<EquipmentItem?> GetByIdAsync(int id);
        Task CreateAsync(EquipmentItem item);
        Task UpdateAsync(EquipmentItem item);
        Task DeleteAsync(int id);
    }
}
