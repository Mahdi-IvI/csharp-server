using semester_project.Application.Ports;
using semester_project.Application.Services;
using semester_project.Application.UseCases.Users;
using semester_project.Infrastructure;

namespace semester_project.Presentation;

public static class App
{
    public static RegisterUserHandler RegisterUser { get; private set; } = null!;
    public static LoginUserHandler    LoginUser    { get; private set; } = null!;

    public static void Configure()
    {
        ICredentialStore store = new Infrastructure.Database(); // shared in-memory list
        ITokenService tokenService = new StaticTokenService();  // "{username}-mrpToken"

        RegisterUser = new RegisterUserHandler(store, tokenService);
        LoginUser    = new LoginUserHandler(store, tokenService);
    }
}