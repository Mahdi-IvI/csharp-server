namespace semester_project.Presentation.Http.Contracts.Auth;

public class TokenResponse
{
    public TokenResponse(string token) { Token = token; }
    public string Token { get; }
}