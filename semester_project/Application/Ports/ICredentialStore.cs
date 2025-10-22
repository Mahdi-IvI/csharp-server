namespace semester_project.Application.Ports;

public interface ICredentialStore
{
    Task<bool> ExistsAsync(string username);
    Task AddAsync(string username, string password);
    Task<bool> ValidateAsync(string username, string password);
}