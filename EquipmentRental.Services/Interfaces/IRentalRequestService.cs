using EquipmentRental.Data.Models;

namespace EquipmentRental.Services.Interfaces
{
    /// <summary>Service interface for managing rental requests.</summary>
    public interface IRentalRequestService
    {
        Task<IEnumerable<RentalRequest>> GetAllAsync();
        Task<IEnumerable<RentalRequest>> GetByUserAsync(string userId);
        Task<RentalRequest?> GetByIdAsync(int id);

        /// <summary>Creates a new rental request with the given equipment items and quantities.</summary>
        Task CreateAsync(RentalRequest request, Dictionary<int, int> equipmentQuantities);

        Task UpdateStatusAsync(int id, string newStatus);
        Task DeleteAsync(int id);

        // Dashboard counts
        Task<int> CountAllAsync();
        Task<int> CountPendingAsync();
    }
}
