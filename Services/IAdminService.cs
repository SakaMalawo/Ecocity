using EcoCity.Models;

namespace EcoCity.Services
{
    public interface IAdminService
    {
        Task<Admin> CreateAdminAsync(string userId, string role, string createdBy);
        Task<Admin> GetAdminByUserIdAsync(string userId);
        Task<bool> IsUserAdminAsync(string userId);
        Task<IEnumerable<Admin>> GetAllAdminsAsync();
        Task<Admin> UpdateAdminAsync(Admin admin);
        Task<bool> DeleteAdminAsync(int adminId);
        Task<Admin> GetAdminByIdAsync(int adminId);
        Task UpdateAdminStatsAsync(int adminId, string actionType);
    }
}
