using semester_project.Application.Ports;
using semester_project.Domain.Entities;

namespace semester_project.Infrastructure;

public sealed class Database : ICredentialStore
{
    private readonly List<User> _users = new();

    public Task<bool> ExistsAsync(string? username)
    {
        if (username is null) return Task.FromResult(false);


        return Task.FromResult(
            _users.Any(u => string.Equals(u.UserName, username, StringComparison.OrdinalIgnoreCase))
        );
    }

    public Task AddAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Task.CompletedTask;

        var id = Guid.NewGuid().ToString("N");


        if (!_users.Any(u => string.Equals(u.UserName, username, StringComparison.OrdinalIgnoreCase)))
        {
            _users.Add(new User(id, username, password));
        }

        return Task.CompletedTask;
    }

    public Task<bool> ValidateAsync(string? username, string? password)
    {
        if (username is null || password is null) return Task.FromResult(false);


        var user = _users.FirstOrDefault(u =>
            string.Equals(u.UserName, username, StringComparison.OrdinalIgnoreCase)
        );
        return Task.FromResult(user is not null && user.Password == password);
    }
}