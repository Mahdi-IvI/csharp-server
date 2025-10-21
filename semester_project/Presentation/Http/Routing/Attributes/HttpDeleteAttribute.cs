namespace semester_project.Presentation.Http.Routing.Attributes;

public sealed class HttpDeleteAttribute : HttpMethodAttribute
{
    public HttpDeleteAttribute(string? template = null) : base("DELETE", template) { }
}