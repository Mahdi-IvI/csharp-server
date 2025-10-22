namespace semester_project.Presentation.Http.Contracts;

public sealed class ApiError
{
    public ApiError(string error, string message)
    {
        Error = error;
        Message = message;
    }
    public string Error { get; }
    public string Message { get; }
}