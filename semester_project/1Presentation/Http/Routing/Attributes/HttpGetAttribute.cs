namespace semester_project._1Presentation.Http.Routing.Attributes;

public sealed class HttpGetAttribute : HttpMethodAttribute
{
    public HttpGetAttribute(string? template = null) : base("GET", template)
    {
    }
}