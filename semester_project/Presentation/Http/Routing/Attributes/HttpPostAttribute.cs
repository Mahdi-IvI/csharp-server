namespace semester_project.Presentation.Http.Routing.Attributes;

public sealed class HttpPostAttribute : HttpMethodAttribute
{
    public HttpPostAttribute(string? template = null) : base("POST", template) { }
}