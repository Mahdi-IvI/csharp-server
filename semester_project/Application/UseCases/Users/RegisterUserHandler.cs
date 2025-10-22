using semester_project.Application.Common;
using semester_project.Application.Ports;
using semester_project.Application.Services;

namespace semester_project.Application.UseCases.Users;

public sealed class RegisterUserHandler
{
    private readonly ICredentialStore _store;
    private readonly ITokenService _tokenService;

    public RegisterUserHandler(ICredentialStore store, ITokenService tokenService)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    public async Task<RegisterUserResult> Handle(RegisterUserInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Username) || string.IsNullOrWhiteSpace(input.Password))
            throw new ArgumentException("Username and password are required.");

        if (await _store.ExistsAsync(input.Username).ConfigureAwait(false))
            throw new UsernameAlreadyExistsException(input.Username);

        await _store.AddAsync(input.Username, input.Password).ConfigureAwait(false);

        var token = _tokenService.Generate(input.Username);
        return new RegisterUserResult(token);
    }
}