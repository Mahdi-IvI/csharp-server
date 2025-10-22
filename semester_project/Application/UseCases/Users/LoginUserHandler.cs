using semester_project.Application.Common;
using semester_project.Application.Ports;
using semester_project.Application.Services;

namespace semester_project.Application.UseCases.Users;

public sealed class LoginUserHandler
{
    private readonly ICredentialStore _store;
    private readonly ITokenService _tokenService;

    public LoginUserHandler(ICredentialStore store, ITokenService tokenService)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    public async Task<LoginUserResult> Handle(LoginUserInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Username) || string.IsNullOrWhiteSpace(input.Password))
            throw new ArgumentException("Username and password are required.");

        var valid = await _store.ValidateAsync(input.Username, input.Password).ConfigureAwait(false);
        if (!valid) throw new InvalidCredentialsException();

        var token = _tokenService.Generate(input.Username);
        return new LoginUserResult(token);
    }
}