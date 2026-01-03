namespace semester_project._1Presentation.Http.Routing.Attributes;

public sealed class HttpPutAttribute : HttpMethodAttribute
{
    public HttpPutAttribute(string? template = null) : base("PUT", template)
    {
    }
}