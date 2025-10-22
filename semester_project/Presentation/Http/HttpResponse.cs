using System.Text;
using System.Text.Json;

namespace semester_project.Presentation.Http;

public class HttpResponse
{
    private readonly System.Net.HttpListenerResponse _inner;
    private bool _hasBody;

    internal HttpResponse(System.Net.HttpListenerResponse inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _inner.StatusCode = 204; // default for empty response
    }

    public int StatusCode
    {
        get => _inner.StatusCode;
        set => _inner.StatusCode = value;
    }

    public void SetHeader(string name, string value) => _inner.Headers[name] = value;

    public async Task WriteTextAsync(string content, int statusCode = 200,
        string contentType = "text/plain; charset=utf-8")
    {
        _inner.StatusCode = statusCode;
        _inner.ContentType = contentType;

        var bytes = Encoding.UTF8.GetBytes(content ?? string.Empty);
        _inner.ContentLength64 = bytes.Length;
        _hasBody = true;

        await _inner.OutputStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
    }

    public async Task WriteJsonAsync(object? payload, int statusCode = 200, JsonSerializerOptions? options = null)
    {
        _inner.StatusCode = statusCode;
        _inner.ContentType = "application/json; charset=utf-8";

        var json = JsonSerializer.Serialize(payload, options ?? new JsonSerializerOptions { WriteIndented = false });
        var bytes = Encoding.UTF8.GetBytes(json);
        _inner.ContentLength64 = bytes.Length;
        _hasBody = true;

        await _inner.OutputStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
    }

    internal async Task FinalizeAsync()
    {
        // If the handler wrote nothing, keep the default 204 No Content.
        if (!_hasBody)
        {
            _inner.ContentLength64 = 0;
        }

        // Flush and close
        try
        {
            await _inner.OutputStream.FlushAsync().ConfigureAwait(false);
        }
        finally
        {
            _inner.OutputStream.Close();
        }
    }
}