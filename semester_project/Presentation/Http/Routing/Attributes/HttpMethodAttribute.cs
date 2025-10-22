namespace semester_project.Presentation.Http.Routing.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public abstract class HttpMethodAttribute : Attribute
{
    public string Method { get; }
    public string? Name { get; }

    protected HttpMethodAttribute(string method, string? name = null)
    {
        Method = method.ToUpperInvariant();
        Name = name is null ? null : RouteAttribute.Normalize(name);
    }
}