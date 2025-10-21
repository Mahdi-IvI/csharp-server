namespace semester_project.Presentation.Http.Routing.Attributes;

public sealed class HttpPutAttribute : HttpMethodAttribute
{
    public HttpPutAttribute(string? template = null) : base("PUT", template) { }
}