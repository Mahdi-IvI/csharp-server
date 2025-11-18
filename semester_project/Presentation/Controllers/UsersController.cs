using System.Text.Json;
using semester_project.Application.UseCases.Users;
using semester_project.Presentation.Http;
using semester_project.Presentation.Http.Contracts;
using semester_project.Presentation.Http.Contracts.Auth;
using semester_project.Presentation.Http.Contracts.Users;
using semester_project.Presentation.Http.Routing.Attributes;

namespace semester_project.Presentation.Controllers;

[Route("users")]
public sealed class UsersController
{
    private static readonly JsonSerializerOptions Json =
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true, AllowTrailingCommas = true, };

    // POST /users/register
    [HttpPost("register")]
    public async Task Register(HttpRequest req, HttpResponse res)
    {
        var body = await req.ReadBodyAsStringAsync().ConfigureAwait(false);
        var dto = JsonSerializer.Deserialize<RegisterRequest>(body, Json);

        if (string.IsNullOrWhiteSpace(dto?.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            await res.WriteJsonAsync(new ApiError("BadRequest", "Username and password are required."), 400);
            return;
        }

        try
        {
            var result = await App.RegisterUser.Handle(
                new RegisterUserInput(dto.Username!, dto.Password!)
            );
            await res.WriteJsonAsync(new TokenResponse(result.Token));
        }
        catch (InvalidOperationException)
        {
            // 409: Conflict
            await res.WriteJsonAsync(new ApiError("UserExists", "User Exist!"), 409);
        }
        catch (ArgumentException)
        {
            res.StatusCode = 400;
        }
        catch (Exception)
        {
            res.StatusCode = 500;
        }
    }

    // POST /users/login
    [HttpPost("login")]
    public async Task Login(HttpRequest req, HttpResponse res)
    {
        var body = await req.ReadBodyAsStringAsync().ConfigureAwait(false);
        var dto = JsonSerializer.Deserialize<LoginRequest>(body, Json);

        if (string.IsNullOrWhiteSpace(dto?.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            await res.WriteJsonAsync(new ApiError("BadRequest", "Username and password are required."), 400);
            return;
        }

        try
        {
            var result = await App.LoginUser.Handle(
                new LoginUserInput(dto.Username!, dto.Password!)
            );
            await res.WriteJsonAsync(new TokenResponse(result.Token));
        }
        catch (UnauthorizedAccessException)
        {
            res.StatusCode = 401;
        }
        catch (ArgumentException)
        {
            res.StatusCode = 400;
        }
        catch (Exception)
        {
            res.StatusCode = 500;
        }
    }

    // GET /api/users/{id}/profile
    [HttpGet("{id}/profile")]
    public async Task GetProfile(HttpRequest req, HttpResponse res)
    {
        if (!req.TryGetRouteValue("id", out var idRaw) || !long.TryParse(idRaw, out var userId))
        {
            res.StatusCode = 400;
            return;
        }

        try
        {
            var user = await App.GetUserProfile.HandleAsync(userId).ConfigureAwait(false);

            var resp = new UserProfileResponse
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FavoriteGenre = user.FavoriteGenre?.ToString(),
                CreatedAt = user.CreatedAt
            };

            await res.WriteJsonAsync(resp).ConfigureAwait(false);
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            res.StatusCode = 404; // Not Found
        }
        catch (Exception)
        {
            res.StatusCode = 500; // Internal Server Error
        }
    }

    private static bool TryGetBearerToken(HttpRequest req, out string token)
    {
        token = string.Empty;
        var hdr = req.GetHeader("Authorization");
        if (hdr is null) return false;
        if (!hdr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return false;
        token = hdr.Substring("Bearer ".Length).Trim();
        return token.Length > 0;
    }


    // PUT /api/users/{id}/profile
    [HttpPut("{id}/profile")]
    public async Task UpdateProfile(HttpRequest req, HttpResponse res)
    {
        // 1) require bearer token
        if (!TryGetBearerToken(req, out var bearer))
        {
            res.StatusCode = 401;
            return;
        }

        if (!App.Tokens.TryGetUsername(bearer, out var username))
        {
            res.StatusCode = 401;
            return;
        }

        // 2) route id must match token's user id
        if (!req.TryGetRouteValue("id", out var idRaw) || !long.TryParse(idRaw, out var routeId))
        {
            res.StatusCode = 400;
            return;
        }

        try
        {
            var tokenUser = await App.GetUserByUsername.HandleAsync(username).ConfigureAwait(false);
            if (tokenUser.Id != routeId)
            {
                res.StatusCode = 403; // forbidden - cannot edit others
                return;
            }

            var body = await req.ReadBodyAsStringAsync().ConfigureAwait(false);
            var dto = JsonSerializer.Deserialize<UpdateUserProfileRequest>(body, Json);

            await App.UpdateUserProfile.HandleAsync(
                new UpdateUserProfileInput(
                    tokenUser.Id,
                    dto?.Email,
                    dto?.FirstName,
                    dto?.LastName,
                    dto?.FavoriteGenre
                )
            ).ConfigureAwait(false);

            res.StatusCode = 204; // No Content
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            res.StatusCode = 401; // token user not found
        }
        catch (Npgsql.NpgsqlException ex) when (ex.SqlState == "23505")
        {
            res.StatusCode = 409; // duplicate email
        }
        catch (ArgumentException)
        {
            res.StatusCode = 400;
        }
        catch (Exception)
        {
            res.StatusCode = 500;
        }
    }
}