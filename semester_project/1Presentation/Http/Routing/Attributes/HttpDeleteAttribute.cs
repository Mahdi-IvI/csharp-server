namespace semester_project._1Presentation.Http.Routing.Attributes;

public sealed class HttpDeleteAttribute : HttpMethodAttribute
{
    public HttpDeleteAttribute(string? template = null) : base("DELETE", template)
    {
    }
}