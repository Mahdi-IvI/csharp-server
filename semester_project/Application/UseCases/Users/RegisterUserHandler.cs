using semester_project.Application.Repositories;
using semester_project.Application.Services;
using semester_project.Domain.Entities;

namespace semester_project.Application.UseCases.Users;

public sealed class RegisterUserHandler
{
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _users;

    public RegisterUserHandler(IUserRepository users, ITokenService tokenService)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    public async Task<RegisterUserResult> Handle(RegisterUserInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Username) || string.IsNullOrWhiteSpace(input.Password))
            throw new ArgumentException("Username and password are required.");

        if (await _users.ExistsByUsernameAsync(input.Username))
            throw new InvalidOperationException("Username already exists.");


        var user = new User
        (
            0,
            input.Username, input.Password, null, null, null, DateTime.UtcNow
        );
        long newId = 0;
        try
        {
            newId = await _users.AddAsync(user);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        user.Id = newId;


        var token = _tokenService.Generate(input.Username);
        return new RegisterUserResult(token);
    }
}