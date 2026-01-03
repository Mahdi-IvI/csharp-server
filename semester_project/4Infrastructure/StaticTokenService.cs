using semester_project._2Application.Interfaces;

namespace semester_project._4Infrastructure;

public sealed class StaticTokenService : ITokenService
{
    private const string Suffix = "-mrpToken";

    public string Generate(string username) => $"{username}-mrpToken";

    public bool TryGetUsername(string token, out string username)
    {
        username = string.Empty;
        if (string.IsNullOrWhiteSpace(token) || !token.EndsWith(Suffix)) return false;
        var u = token.Substring(0, token.Length - Suffix.Length);
        if (string.IsNullOrWhiteSpace(u)) return false;
        username = u;
        return true;
    }
}