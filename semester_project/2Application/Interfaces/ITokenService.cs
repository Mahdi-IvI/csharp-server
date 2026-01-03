namespace semester_project._2Application.Interfaces;

public interface ITokenService
{
    string Generate(string username);

    bool TryGetUsername(string token, out string username);
}