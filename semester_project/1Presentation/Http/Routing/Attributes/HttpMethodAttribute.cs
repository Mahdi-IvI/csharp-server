namespace semester_project._1Presentation.Http.Routing.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public abstract class HttpMethodAttribute(string method, string? name = null) : Attribute
{
    public string Method { get; } = method.ToUpperInvariant();
    public string? Name { get; } = name is null ? null : RouteAttribute.Normalize(name);
}