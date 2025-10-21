using System.Text.Json;
using semester_project.Presentation.Http;
using semester_project.Presentation.Http.Contracts.Auth;
using semester_project.Presentation.Http.Routing.Attributes;

namespace semester_project.Presentation.Controllers;

[Route("users")]
public sealed class UsersController
{
    private static readonly JsonSerializerOptions Json =
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    // POST /auth/register
    [HttpPost("register")]
    public async Task Register(HttpRequest req, HttpResponse res)
    {
        var body = await req.ReadBodyAsStringAsync().ConfigureAwait(false);
        var dto = JsonSerializer.Deserialize<RegisterRequest>(body, Json);

        if (string.IsNullOrWhiteSpace(dto?.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            res.StatusCode = 400; // Bad Request
            return; // empty body by design
        }

        // No DB: we just return a token based on username.
        var token = $"{dto.Username}-mrpToken";
        await res.WriteJsonAsync(new TokenResponse(token)).ConfigureAwait(false);
    }

    // POST /auth/login
    [HttpPost("login")]
    public async Task Login(HttpRequest req, HttpResponse res)
    {
        var body = await req.ReadBodyAsStringAsync().ConfigureAwait(false);
        var dto = JsonSerializer.Deserialize<LoginRequest>(body, Json);

        if (string.IsNullOrWhiteSpace(dto?.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            res.StatusCode = 400; // Bad Request
            return;
        }

        var token = $"{dto.Username}-mrpToken";
        await res.WriteJsonAsync(new TokenResponse(token)).ConfigureAwait(false);
    }
}