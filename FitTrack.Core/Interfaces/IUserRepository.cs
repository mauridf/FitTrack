using FitTrack.Core.Entities;

namespace FitTrack.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> DeleteAsync(string id);
    Task<bool> EmailExistsAsync(string email);
}