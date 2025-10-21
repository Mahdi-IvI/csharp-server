using semester_project.Domain.Entities;

namespace semester_project.Application.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> ExistsByUsernameAsync(string username);
    Task AddAsync(User user);
}