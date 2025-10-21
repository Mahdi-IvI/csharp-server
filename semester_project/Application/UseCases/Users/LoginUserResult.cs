namespace semester_project.Application.UseCases.Users;

public sealed class LoginUserResult
{
    public LoginUserResult(string token) { Token = token; }
    public string Token { get; }
}