using semester_project._2Application.Interfaces;
using semester_project._3Domain.Entities;

namespace semester_project._2Application.UseCases.Users;

public class GetUserByUsernameHandler(IUserRepository users)
{
    public async Task<User> HandleAsync(string username)
    {
        var user = await users.GetByUsernameAsync(username);
        if (user is null) throw new KeyNotFoundException("User not found");
        return user;
    }
}