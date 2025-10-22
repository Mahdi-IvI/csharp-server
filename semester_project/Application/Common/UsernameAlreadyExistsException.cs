namespace semester_project.Application.Common;

public sealed class UsernameAlreadyExistsException : Exception
{
    public UsernameAlreadyExistsException(string username)
        : base($"Username '{username}' already exists.") { }
}