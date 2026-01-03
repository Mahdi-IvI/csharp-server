using semester_project._1Presentation;
using semester_project._1Presentation.Http;
using semester_project._1Presentation.Http.Routing;

namespace semester_project;

class Program
{
    static async Task Main(string[] args)
    {
        App.Configure();

        var router = Router.Discover(
            basePath: "api"
        );
        router.DumpRoutes();

        var server = new Server(router, "http://localhost:8080/");
        server.Start();

        Console.WriteLine("Press ENTER to stop.");
        Console.ReadLine();
        await server.Stop();
    }
}