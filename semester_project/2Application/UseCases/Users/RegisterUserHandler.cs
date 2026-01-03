using semester_project._2Application.Interfaces;
using semester_project._3Domain.Entities;

namespace semester_project._2Application.UseCases.Users;

public sealed class RegisterUserHandler(IUserRepository users, ITokenService tokenService)
{
    public async Task<RegisterUserResult> Handle(RegisterUserInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Username) || string.IsNullOrWhiteSpace(input.Password))
            throw new ArgumentException("Username and password are required.");

        if (await users.ExistsByUsernameAsync(input.Username))
            throw new InvalidOperationException("Username already exists.");


        var user = new User
        (
            0, input.Username, input.Password, null, null, null, DateTime.Now
        );
        long newId = 0;
        try
        {
            newId = await users.AddAsync(user);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        user = user with{Id = newId};


        var token = tokenService.Generate(input.Username);
        return new RegisterUserResult(token);
    }
}