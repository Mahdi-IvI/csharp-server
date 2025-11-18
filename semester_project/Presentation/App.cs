using semester_project.Application.Ports;
using semester_project.Application.Repositories;
using semester_project.Application.Services;
using semester_project.Application.UseCases.Media;
using semester_project.Application.UseCases.Users;
using semester_project.Infrastructure;
using semester_project.Infrastructure.data;
using semester_project.Infrastructure.repositories;

namespace semester_project.Presentation;

public static class App
{
    public static RegisterUserHandler RegisterUser { get; private set; } = null!;
    public static LoginUserHandler LoginUser { get; private set; } = null!;
    public static GetUserProfileHandler GetUserProfile { get; private set; } = null!;
    public static UpdateUserProfileHandler UpdateUserProfile { get; private set; } = null!;
    public static GetUserByUsernameHandler   GetUserByUsername   { get; private set; } = null!;
    public static ITokenService Tokens { get; private set; } = null!; // expose token service
    public static CreateMediaHandler CreateMedia { get; private set; } = null!;
    public static DeleteMediaHandler DeleteMedia { get; private set; } = null!;



    public static void Configure()
    {
        var connStr =
            Environment.GetEnvironmentVariable("MRP_DB") ??
            "Host=localhost;Port=5432;Database=mrp;Username=mrp_user;Password=mrp_password";

        var factory = new PostgresConnectionFactory(connStr);
        IUserRepository usersRepo = new PostgresUserRepository(factory);
        ITokenService tokenService = new StaticTokenService(); // "{username}-mrpToken"
        IMediaRepository mediaRepo = new PostgresMediaRepository(factory);


        RegisterUser = new RegisterUserHandler(usersRepo, tokenService);
        LoginUser = new LoginUserHandler(usersRepo, tokenService);
        GetUserProfile = new GetUserProfileHandler(usersRepo);
        UpdateUserProfile = new UpdateUserProfileHandler(usersRepo);
        GetUserByUsername = new GetUserByUsernameHandler(usersRepo);
        Tokens = tokenService;
        CreateMedia = new CreateMediaHandler(mediaRepo);
        DeleteMedia  = new DeleteMediaHandler(mediaRepo);
    }
}