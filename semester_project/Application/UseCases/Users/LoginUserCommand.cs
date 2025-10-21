namespace semester_project.Application.UseCases.Users;

public sealed class LoginUserCommand
{
    public LoginUserCommand(string username, string password)
    {
        Username = username;
        Password = password;
    }

    public string Username { get; }
    public string Password { get; }
}