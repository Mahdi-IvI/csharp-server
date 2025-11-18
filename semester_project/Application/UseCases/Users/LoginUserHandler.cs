using semester_project.Application.Repositories;
using semester_project.Application.Services;

namespace semester_project.Application.UseCases.Users;

public sealed class LoginUserHandler
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokenService;

    public LoginUserHandler(IUserRepository users, ITokenService tokenService)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    public async Task<LoginUserResult> Handle(LoginUserInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Username) || string.IsNullOrWhiteSpace(input.Password))
            throw new ArgumentException("Username and password are required.");

        try
        {
            var user = await _users.GetByUsernameAsync(input.Username);
            if (user is null) throw new UnauthorizedAccessException("Invalid credentials.");

            Console.WriteLine(user.Password);
            Console.WriteLine(input.Password);
            Console.WriteLine(input.Password.Equals(user.Password));
            if (!input.Password.Equals(user.Password))
                throw new UnauthorizedAccessException("Invalid credentials.");

            var token = _tokenService.Generate(input.Username);
            return new LoginUserResult(token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new LoginUserResult("token");
    }
}