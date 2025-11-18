using semester_project.Domain.Entities;
using semester_project.Domain.Enums;

namespace semester_project.Application.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<long> AddAsync(User user);
    
    Task<bool> UpdateProfileAsync(long id, string? email, string? firstName, string? lastName, Genre? favoriteGenre);

}