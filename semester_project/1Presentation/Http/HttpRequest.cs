using System.Collections.Specialized;
using System.Text;

namespace semester_project._1Presentation.Http;

public class HttpRequest
{
    private readonly System.Net.HttpListenerRequest _inner;

    private Dictionary<string, string> _routeValues =
        new(StringComparer.OrdinalIgnoreCase);

    internal HttpRequest(System.Net.HttpListenerRequest inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public string Method => _inner.HttpMethod; // "GET", "POST", "PUT", "DELETE"
    public Uri Url => _inner.Url!;
    public string Path => _inner.Url!.AbsolutePath; // "/api/users",
    public NameValueCollection Query => _inner.QueryString; // ?title=incep&genre=sci-fi&sortBy=score
    public NameValueCollection Headers => _inner.Headers;

    public Stream BodyStream => _inner.InputStream;

    public async Task<string> ReadBodyAsStringAsync()
    {
        if (!_inner.HasEntityBody) return string.Empty;

        using var reader =
            new StreamReader(_inner.InputStream, _inner.ContentEncoding ?? Encoding.UTF8, leaveOpen: true);
        var content = await reader.ReadToEndAsync().ConfigureAwait(false);

        // Reset the stream position if the caller needs to re-read (when possible).
        if (_inner.InputStream.CanSeek)
            _inner.InputStream.Position = 0;

        return content;
    }

    public string? GetHeader(string name) => _inner.Headers[name];

    public override string ToString()
        => $"{Method} {Path}{(_inner.Url!.Query ?? string.Empty)}";

    public IReadOnlyDictionary<string, string> RouteValues => _routeValues;

    internal void SetRouteValues(Dictionary<string, string> values)
        => _routeValues = values ?? new(StringComparer.OrdinalIgnoreCase);

    public bool TryGetRouteValue(string key, out string value)
        => _routeValues.TryGetValue(key, out value);
}