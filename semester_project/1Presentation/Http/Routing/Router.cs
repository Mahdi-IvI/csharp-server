using System.Reflection;
using semester_project._1Presentation.Http.Routing.Attributes;

namespace semester_project._1Presentation.Http.Routing;

public sealed class Router
{
    private readonly List<RouteDefinition> _routes = new();
    private readonly string _basePath; // base path: api/

    private Router(string? basePath)
    {
        _basePath = (basePath ?? string.Empty).Trim('/').ToLowerInvariant();
    }

    public static Router Discover(string? basePath = null)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        var router = new Router(basePath);


        foreach (var type in assembly.GetTypes())
        {
            var routeAttr = type.GetCustomAttribute<RouteAttribute>();
            if (routeAttr is null) continue;

            var baseTemplate = routeAttr.Name;

            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public |
                                                   BindingFlags.DeclaredOnly))
            {
                var httpAttr = method.GetCustomAttributes<HttpMethodAttribute>().FirstOrDefault();
                if (httpAttr is null) continue;

                var fullTemplate = Combine(baseTemplate, httpAttr.Name);
                router._routes.Add(new RouteDefinition(httpAttr.Method, fullTemplate, type, method));
            }
        }


        return router;
    }

    // print Routes list for debugging
    public void DumpRoutes()
    {
        Console.WriteLine("=== Discovered routes ===");
        foreach (var r in _routes)
            Console.WriteLine(
                $"{r.HttpMethod,-6} /{(string.IsNullOrEmpty(_basePath) ? "" : _basePath + "/")}{r.URL,-30}  -> {r.ControllerType.Name}.{r.Method.Name}()");
    }

    public bool TryMatch(string httpMethod, string path, out RouteDefinition? route,
        out Dictionary<string, string> values)
    {
        httpMethod = httpMethod.ToUpperInvariant();
        values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        route = null;

        // Normalize incoming path and strip basePath if configured
        var norm = path.Trim('/');

        if (!string.IsNullOrEmpty(_basePath))
        {
            var bp = _basePath + "/";
            if (norm.StartsWith(bp, StringComparison.OrdinalIgnoreCase))
            {
                norm = norm.Substring(bp.Length); // drop "api/"
            }
            else if (string.Equals(norm, _basePath, StringComparison.OrdinalIgnoreCase))
            {
                norm = string.Empty;
            }
            else
            {
                // Incoming path doesn't start with basePath at all
                return false;
            }
        }

        foreach (var r in _routes.Where(r => r.HttpMethod == httpMethod))
        {
            if (r.Path.TryMatch(norm, out var rv))
            {
                values = rv;
                route = r;
                return true;
            }
        }

        return false;
    }

    private static string Combine(string baseTemplate, string? methodTemplate)
        => string.IsNullOrEmpty(methodTemplate) ? baseTemplate : $"{baseTemplate}/{methodTemplate}".Trim('/');
}