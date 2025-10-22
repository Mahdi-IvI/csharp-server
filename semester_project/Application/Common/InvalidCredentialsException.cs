namespace semester_project.Application.Common;


public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("Invalid username or password.") { }
}