using semester_project.Application.Services;

namespace semester_project.Application.UseCases.Users;

public sealed class LoginUserHandler
{
    private readonly ITokenService _tokenService;

    public LoginUserHandler(ITokenService tokenService)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    public Task<LoginUserResult> HandleAsync(LoginUserCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password))
            throw new ArgumentException("Username and password are required.");

        var token = _tokenService.Generate(command.Username);
        return Task.FromResult(new LoginUserResult(token));
    }
}