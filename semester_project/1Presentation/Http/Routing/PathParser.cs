namespace semester_project._1Presentation.Http.Routing;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class PathParser
{
    private readonly string[] _segments;
    private readonly Segment[] _compiled;

    private PathParser(string path)
    {
        _segments = Split(path);
        _compiled = _segments.Select(ParseSegment).ToArray();
    }

    public static PathParser Compile(string path) => new PathParser(path);

    public bool TryMatch(string path, out Dictionary<string, string> routeValues)
    {
        routeValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var incoming = Split(path);

        if (incoming.Length != _segments.Length)
            return false;

        for (int i = 0; i < _compiled.Length; i++)
        {
            var seg = _compiled[i];
            var val = incoming[i];

            if (seg.IsParameter)
            {
                routeValues[seg.ParamName!] = val;
            }
            else
            {
                if (!string.Equals(seg.Literal!, val, StringComparison.OrdinalIgnoreCase))
                    return false;
            }
        }

        return true;
    }

    private static string[] Split(string p)
        => p.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static Segment ParseSegment(string s)
    {
        if (s.StartsWith("{") && s.EndsWith("}") && s.Length > 2)
            return new Segment { IsParameter = true, ParamName = s.Substring(1, s.Length - 2) };

        return new Segment { IsParameter = false, Literal = s.ToLowerInvariant() };
    }

    private readonly struct Segment
    {
        public bool IsParameter { get; init; }
        public string? ParamName { get; init; }
        public string? Literal { get; init; }
    }
}