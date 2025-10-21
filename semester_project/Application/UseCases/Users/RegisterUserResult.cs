namespace semester_project.Application.UseCases.Users;

public sealed class RegisterUserResult
{
    public RegisterUserResult(string token) { Token = token; }
    public string Token { get; }
}