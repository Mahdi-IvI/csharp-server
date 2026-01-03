using semester_project._2Application.Interfaces;

namespace semester_project._2Application.UseCases.Users;

public sealed class LoginUserHandler(IUserRepository users, ITokenService tokenService)
{
    public async Task<LoginUserResult> Handle(LoginUserInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Username) || string.IsNullOrWhiteSpace(input.Password))
            throw new ArgumentException("Username and password are required.");

        try
        {
            var user = await users.GetByUsernameAsync(input.Username);
            if (user is null) throw new UnauthorizedAccessException("Invalid credentials.");

            if (!input.Password.Equals(user.Password))
                throw new UnauthorizedAccessException("Invalid credentials.");

            var token = tokenService.Generate(input.Username);
            return new LoginUserResult(token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new LoginUserResult("token");
    }
}