using semester_project._2Application.Interfaces;
using semester_project._3Domain.Entities;

namespace semester_project._2Application.UseCases.Users;

public class GetUserProfileHandler(IUserRepository users)
{
    public async Task<User> HandleAsync(long userId)
    {
        var user = await users.GetByIdAsync(userId);
        if (user is null) throw new KeyNotFoundException("User not found");
        return user;
    }
}