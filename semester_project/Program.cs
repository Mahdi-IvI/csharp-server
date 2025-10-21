using System.Reflection;
using semester_project.Presentation;
using semester_project.Presentation.Http;
using semester_project.Presentation.Http.Routing;

namespace semester_project;

class Program
{
    static async Task Main(string[] args)
    {
        App.Configure();

        var router = Router.Discover(
            basePath: "api",
            assemblies: new[] { Assembly.GetExecutingAssembly() }
        );
        router.DumpRoutes();

        var server = new Server(router, "http://localhost:8080/");
        server.Start();

        Console.WriteLine("Press ENTER to stop.");
        Console.ReadLine();
        await server.StopAsync();
    }
}