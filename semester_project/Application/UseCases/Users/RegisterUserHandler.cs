using semester_project.Application.Services;

namespace semester_project.Application.UseCases.Users;

public sealed class RegisterUserHandler
{
    private readonly ITokenService _tokenService;

    public RegisterUserHandler(ITokenService tokenService)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    public Task<RegisterUserResult> HandleAsync(RegisterUserCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password))
            throw new ArgumentException("Username and password are required.");

        var token = _tokenService.Generate(command.Username);
        return Task.FromResult(new RegisterUserResult(token));
    }
}