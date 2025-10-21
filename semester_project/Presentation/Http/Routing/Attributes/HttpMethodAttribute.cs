namespace semester_project.Presentation.Http.Routing.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public abstract class HttpMethodAttribute : Attribute
{
    public string Method { get; }
    public string? Template { get; }

    protected HttpMethodAttribute(string method, string? template = null)
    {
        Method = method.ToUpperInvariant();
        Template = template is null ? null : RouteAttribute.Normalize(template);
    }
}