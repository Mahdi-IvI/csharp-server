namespace semester_project.Presentation.Http.Routing;

using System;
using System.Reflection;
using System.Threading.Tasks;

public sealed class RouteDefinition
{
    public string HttpMethod { get; }
    public string URL { get; }
    public PathParser Path { get; }
    public Type ControllerType { get; }
    public MethodInfo Method { get; }

    public RouteDefinition(string httpMethod, string url, Type controllerType, MethodInfo method)
    {
        HttpMethod = httpMethod;
        URL = url;
        Path = PathParser.Compile(url);
        ControllerType = controllerType;
        Method = method;
    }

    public async Task InvokeAsync(object controllerInstance, HttpRequest req, HttpResponse res)
    {
        // Minimal injection: pass HttpRequest / HttpResponse if parameters declare them; otherwise call parameterless.
        var parameters = Method.GetParameters();
        object?[] args;
        

        if (parameters.Length == 2 &&
            parameters[0].ParameterType == typeof(HttpRequest) &&
            parameters[1].ParameterType == typeof(HttpResponse))
        {
            args = new object?[] { req, res };
        }
        else if (parameters.Length == 0)
        {
            args = Array.Empty<object?>();
        }
        else
        {
            throw new InvalidOperationException(
                $"Unsupported action signature: {Method.DeclaringType?.Name}.{Method.Name}");
        }

        var result = Method.Invoke(controllerInstance, args);

        if (result is Task t)
            await t.ConfigureAwait(false);
    }
}