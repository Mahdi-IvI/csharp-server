using semester_project.Application.Services;

namespace semester_project.Infrastructure;

public sealed class StaticTokenService : ITokenService
{
    public string Generate(string username) => $"{username}-mrpToken";
}