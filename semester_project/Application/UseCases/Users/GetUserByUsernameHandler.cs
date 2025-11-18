using semester_project.Application.Repositories;
using semester_project.Domain.Entities;

namespace semester_project.Application.UseCases.Users;

public class GetUserByUsernameHandler
{
    private readonly IUserRepository _users;

    public GetUserByUsernameHandler(IUserRepository users) => _users = users;

    public async Task<User> HandleAsync(string username)
    {
        var user = await _users.GetByUsernameAsync(username);
        if (user is null) throw new KeyNotFoundException("User not found");
        return user;
    }
}