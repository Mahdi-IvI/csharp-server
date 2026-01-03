namespace semester_project._1Presentation.Http.Routing.Attributes;

public sealed class HttpPostAttribute : HttpMethodAttribute
{
    public HttpPostAttribute(string? template = null) : base("POST", template)
    {
    }
}