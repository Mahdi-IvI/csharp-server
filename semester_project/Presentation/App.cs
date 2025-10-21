using semester_project.Application.UseCases.Users;
using semester_project.Infrastructure;

namespace semester_project.Presentation;

public static class App
{
    public static RegisterUserHandler RegisterUser { get; private set; } = null!;
    public static LoginUserHandler    LoginUser    { get; private set; } = null!;

    public static void Configure()
    {
        var tokenService = new StaticTokenService();

        RegisterUser = new RegisterUserHandler(tokenService);
        LoginUser    = new LoginUserHandler(tokenService);
    }
}