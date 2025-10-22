using System.Net;
using semester_project.Presentation.Http.Routing;

namespace semester_project.Presentation.Http;

public sealed class Server : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly Router _router;
    private Task? _loopTask;
    private CancellationTokenSource? _cts;

    public Server(Router router, params string[] prefixes)
    {
        _router = router ?? throw new ArgumentNullException(nameof(router));

        if (prefixes == null || prefixes.Length == 0)
            throw new ArgumentException("At least one HTTP prefix is required, e.g., http://localhost:8080/");

        foreach (var p in prefixes)
            _listener.Prefixes.Add(NormalizePrefix(p));
    }

    public void Start()
    {
        if (_cts != null) throw new InvalidOperationException("Server already started.");

        _cts = new CancellationTokenSource();
        _listener.Start();
        _loopTask = Task.Run(() => AcceptLoopAsync(_cts.Token));
        Console.WriteLine($"[HttpServer] Listening on: {string.Join(", ", _listener.Prefixes)}");
    }

    public async Task Stop()
    {
        if (_cts == null) return;

        _cts.Cancel();
        _listener.Stop();

        try
        {
            if (_loopTask != null) await _loopTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
            _loopTask = null;
        }
    }

    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var ctx = await _listener.GetContextAsync().ConfigureAwait(false);
                _ = Task.Run(() => HandleAsync(ctx), ct);
            }
            catch (HttpListenerException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[HttpServer] AcceptLoop error: {ex}");
            }
        }
    }

    private async Task HandleAsync(HttpListenerContext ctx)
    {
        var req = new HttpRequest(ctx.Request);
        var res = new HttpResponse(ctx.Response);

        try
        {
            // Try to match route
            if (_router.TryMatch(req.Method, req.Path, out var route, out var _))
            {
                var controller = Activator.CreateInstance(route!.ControllerType)
                                 ?? throw new InvalidOperationException(
                                     $"Cannot create controller {route.ControllerType.Name}");

                await route.InvokeAsync(controller, req, res).ConfigureAwait(false);
            }
            else
            {
                res.StatusCode = 404; // Not Found
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[HttpServer] Unhandled error: {ex}");
            res.StatusCode = 500;
        }
        finally
        {
            await res.FinalizeAsync().ConfigureAwait(false);
        }
    }

    private static string NormalizePrefix(string prefix)
        => prefix.EndsWith("/") ? prefix : prefix + "/";

    public void Dispose()
    {
        _listener.Close();
        _cts?.Dispose();
    }
}