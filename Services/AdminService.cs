using EcoCity.Models;
using EcoCity.Data;
using Microsoft.EntityFrameworkCore;

namespace EcoCity.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Admin> CreateAdminAsync(string userId, string role, string createdBy)
        {
            var admin = new Admin
            {
                UserId = userId,
                Role = role,
                Department = "Administration",
                Permissions = "Full",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
            return admin;
        }

        public async Task<Admin> GetAdminByUserIdAsync(string userId)
        {
            return await _context.Admins
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<bool> IsUserAdminAsync(string userId)
        {
            return await _context.Admins
                .AnyAsync(a => a.UserId == userId && a.IsActive);
        }

        public async Task<IEnumerable<Admin>> GetAllAdminsAsync()
        {
            return await _context.Admins
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Admin> UpdateAdminAsync(Admin admin)
        {
            _context.Admins.Update(admin);
            await _context.SaveChangesAsync();
            return admin;
        }

        public async Task<bool> DeleteAdminAsync(int adminId)
        {
            var admin = await _context.Admins.FindAsync(adminId);
            if (admin != null)
            {
                _context.Admins.Remove(admin);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<Admin> GetAdminByIdAsync(int adminId)
        {
            return await _context.Admins
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == adminId);
        }

        public async Task UpdateAdminStatsAsync(int adminId, string actionType)
        {
            var admin = await _context.Admins.FindAsync(adminId);
            if (admin != null)
            {
                switch (actionType.ToLower())
                {
                    case "approve":
                        admin.InitiativesApproved++;
                        break;
                    case "reject":
                        admin.InitiativesRejected++;
                        break;
                    default:
                        admin.ActionsCount++;
                        break;
                }
                admin.LastActionAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
