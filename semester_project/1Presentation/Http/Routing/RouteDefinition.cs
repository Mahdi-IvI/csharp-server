namespace semester_project._1Presentation.Http.Routing;

using System;
using System.Reflection;
using System.Threading.Tasks;

public sealed class RouteDefinition(string httpMethod, string url, Type controllerType, MethodInfo method)
{
    public string HttpMethod { get; } = httpMethod;
    public string URL { get; } = url;
    public PathParser Path { get; } = PathParser.Compile(url);
    public Type ControllerType { get; } = controllerType;
    public MethodInfo Method { get; } = method;

    public async Task InvokeAsync(object controllerInstance, HttpRequest req, HttpResponse res)
    {
        // pass HttpRequest / HttpResponse if parameters declare them; otherwise call parameterless.
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