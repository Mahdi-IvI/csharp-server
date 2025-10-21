namespace semester_project.Presentation.Http.Routing.Attributes;

public sealed class HttpGetAttribute : HttpMethodAttribute
{
    public HttpGetAttribute(string? template = null) : base("GET", template) { }
}