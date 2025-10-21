namespace semester_project.Presentation.Http.Routing.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class RouteAttribute : Attribute
{
    public string Template { get; }

    public RouteAttribute(string template)
    {
        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException("Route template cannot be empty.", nameof(template));

        Template = Normalize(template);
    }

    internal static string Normalize(string t)
        => t.Trim().Trim('/').ToLowerInvariant();
}