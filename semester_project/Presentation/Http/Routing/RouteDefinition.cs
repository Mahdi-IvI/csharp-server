namespace semester_project.Presentation.Http.Routing;

using System;
using System.Reflection;
using System.Threading.Tasks;

public sealed class RouteDefinition
{
    public string HttpMethod { get; }
    public string Template { get; }
    public PathTemplate Path { get; }
    public Type ControllerType { get; }
    public MethodInfo Method { get; }

    public RouteDefinition(string httpMethod, string template, Type controllerType, MethodInfo method)
    {
        HttpMethod = httpMethod;
        Template = template;
        Path = PathTemplate.Compile(template);
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
            // Not supported in this first step; feel free to extend for route values injection later.
            throw new InvalidOperationException(
                $"Unsupported action signature: {Method.DeclaringType?.Name}.{Method.Name}");
        }

        var result = Method.Invoke(controllerInstance, args);

        if (result is Task t)
            await t.ConfigureAwait(false);
    }
}