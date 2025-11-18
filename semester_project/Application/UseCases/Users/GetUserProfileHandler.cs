using semester_project.Application.Repositories;
using semester_project.Domain.Entities;

namespace semester_project.Application.UseCases.Users;

public class GetUserProfileHandler
{
    private readonly IUserRepository _users;

    public GetUserProfileHandler(IUserRepository users) => _users = users;

    public async Task<User> HandleAsync(long userId)
    {
        var user = await _users.GetByIdAsync(userId);
        if (user is null) throw new KeyNotFoundException("User not found");
        return user;
    }
}