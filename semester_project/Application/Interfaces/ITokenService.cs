namespace semester_project.Application.Services;

public interface ITokenService
{
    string Generate(string username);
    
    bool TryGetUsername(string token, out string username);
}