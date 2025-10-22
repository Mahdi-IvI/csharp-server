namespace semester_project.Presentation.Http.Routing.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RouteAttribute : Attribute
{
    public string Name { get; }

    public RouteAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Route Name cannot be empty.", nameof(name));

        Name = Normalize(name);
    }

    internal static string Normalize(string t)
        => t.Trim().Trim('/').ToLowerInvariant();
}