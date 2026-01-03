using System.Text.Json;
using semester_project._2Application.UseCases.Users;
using semester_project._1Presentation.Http;
using semester_project._1Presentation.Http.Contracts;
using semester_project._1Presentation.Http.Contracts.Auth;
using semester_project._1Presentation.Http.Contracts.Favorites;
using semester_project._1Presentation.Http.Contracts.Recommendations;
using semester_project._1Presentation.Http.Contracts.Users;
using semester_project._1Presentation.Http.Routing.Attributes;
using semester_project._2Application.UseCases.Favorites;
using semester_project._2Application.UseCases.Ratings;
using semester_project._2Application.UseCases.Recommendations;

namespace semester_project._1Presentation.Controllers;

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
            (
                Id: user.Id,
                Username: user.Username,
                FirstName: user.FirstName,
                LastName: user.LastName,
                FavoriteGenre: user.FavoriteGenre?.ToString(),
                CreatedAt: user.CreatedAt
            );

            await res.WriteJsonAsync(resp).ConfigureAwait(false);
        }
        catch (KeyNotFoundException)
        {
            res.StatusCode = 404; // Not Found
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
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
        catch (KeyNotFoundException)
        {
            res.StatusCode = 401; // token user not found
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

    // GET /users/{id}/ratings
    [HttpGet("{id}/ratings")]
    public async Task GetRatingHistory(HttpRequest req, HttpResponse res)
    {
        // Auth
        if (!TryGetBearerToken(req, out var bearer) || !App.Tokens.TryGetUsername(bearer, out var username))
        {
            res.StatusCode = 401;
            return;
        }

        // Path id
        if (!req.TryGetRouteValue("id", out var idRaw) || !long.TryParse(idRaw, out var userIdFromPath))
        {
            res.StatusCode = 400;
            return;
        }

        try
        {
            // Resolve requester user
            var requester = await App.GetUserByUsername.HandleAsync(username).ConfigureAwait(false);

            // Security rule: rating history is private (owner-only).
            // If you later add admin, you can allow admin here too.
            if (requester.Id != userIdFromPath)
            {
                res.StatusCode = 403;
                return;
            }

            var items = await App.GetUserRatingHistory.HandleAsync(
                new GetUserRatingHistoryInput(userIdFromPath)
            ).ConfigureAwait(false);

            var response = new UserRatingHistoryResponse(
                items.Select(x => new UserRatingHistoryItemResponse(
                    RatingId: x.RatingId,
                    MediaId: x.MediaId,
                    MediaTitle: x.MediaTitle,
                    Stars: x.Stars,
                    Comment: x.Comment,
                    IsCommentConfirmed: x.IsCommentConfirmed,
                    CreatedAt: x.CreatedAt
                )).ToList()
            );

            await res.WriteJsonAsync(response).ConfigureAwait(false);
        }
        catch (KeyNotFoundException)
        {
            // requester not found (shouldn't happen if token system is consistent)
            res.StatusCode = 401;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await res.WriteJsonAsync(new ApiError("ServerError", "Unexpected error"), 500);
            res.StatusCode = 500;
        }
    }

    // GET /users/{id}/favorites
    [HttpGet("{id}/favorites")]
    public async Task GetFavorites(HttpRequest req, HttpResponse res)
    {
        // Auth
        if (!TryGetBearerToken(req, out var bearer) || !App.Tokens.TryGetUsername(bearer, out var username))
        {
            res.StatusCode = 401;
            return;
        }

        // Path id
        if (!req.TryGetRouteValue("id", out var idRaw) || !long.TryParse(idRaw, out var userIdFromPath))
        {
            res.StatusCode = 400;
            return;
        }

        try
        {
            // requester user
            var requester = await App.GetUserByUsername.HandleAsync(username).ConfigureAwait(false);

            // Owner-only rule
            if (requester.Id != userIdFromPath)
            {
                res.StatusCode = 403;
                return;
            }

            var items = await App.GetUserFavorites.HandleAsync(new GetUserFavoritesInput(userIdFromPath))
                .ConfigureAwait(false);

            var response = new UserFavoritesResponse(
                items.Select(x => new UserFavoriteItemResponse(
                    MediaId: x.MediaId,
                    Title: x.Title,
                    MediaType: x.MediaType,
                    ReleaseYear: x.ReleaseYear,
                    AgeRestriction: x.AgeRestriction,
                    FavoritedAt: x.FavoritedAt
                )).ToList()
            );

            await res.WriteJsonAsync(response).ConfigureAwait(false);
        }
        catch (KeyNotFoundException)
        {
            res.StatusCode = 401;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await res.WriteJsonAsync(new ApiError("ServerError", "Unexpected error"), 500).ConfigureAwait(false);
            res.StatusCode = 500;
        }
    }

    // GET /users/{id}/recommendations?type=genre|content&limit=10
    [HttpGet("{id}/recommendations")]
    public async Task Get(HttpRequest req, HttpResponse res)
    {
        // Auth
        if (!TryGetBearerToken(req, out var bearer) || !App.Tokens.TryGetUsername(bearer, out var username))
        {
            res.StatusCode = 401;
            return;
        }

        // Path id
        if (!req.TryGetRouteValue("id", out var idRaw) || !long.TryParse(idRaw, out var userIdFromPath))
        {
            res.StatusCode = 400;
            return;
        }

        try
        {
            var requester = await App.GetUserByUsername.HandleAsync(username).ConfigureAwait(false);

            // Owner-only
            if (requester.Id != userIdFromPath)
            {
                res.StatusCode = 403;
                return;
            }

            var typeRaw = req.Query["type"];
            if (string.IsNullOrWhiteSpace(typeRaw))
            {
                await res.WriteJsonAsync(new ApiError("BadRequest", "Missing query parameter: type"), 400)
                    .ConfigureAwait(false);
                res.StatusCode = 400;
                return;
            }

            var type = typeRaw.Equals("genre", StringComparison.OrdinalIgnoreCase)
                ? RecommendationType.Genre
                : typeRaw.Equals("content", StringComparison.OrdinalIgnoreCase)
                    ? RecommendationType.Content
                    : (RecommendationType?)null;

            if (type is null)
            {
                await res.WriteJsonAsync(new ApiError("BadRequest", "type must be 'genre' or 'content'"), 400)
                    .ConfigureAwait(false);
                res.StatusCode = 400;
                return;
            }

            var limit = 10;

            var items = await App.GetUserRecommendations.HandleAsync(
                new GetUserRecommendationsInput(userIdFromPath, type.Value, limit)
            ).ConfigureAwait(false);

            var response = new UserRecommendationsResponse(
                items.Select(x => new RecommendationItemResponse(
                    MediaId: x.MediaId,
                    Title: x.Title,
                    MediaType: x.MediaType,
                    ReleaseYear: x.ReleaseYear,
                    AgeRestriction: x.AgeRestriction,
                    Genres: x.Genres,
                    AverageStars: x.AverageStars
                )).ToList()
            );

            await res.WriteJsonAsync(response).ConfigureAwait(false);
        }
        catch (ArgumentException e)
        {
            await res.WriteJsonAsync(new ApiError("BadRequest", e.Message), 400).ConfigureAwait(false);
            res.StatusCode = 400;
        }
        catch (KeyNotFoundException)
        {
            res.StatusCode = 401;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await res.WriteJsonAsync(new ApiError("ServerError", "Unexpected error"), 500).ConfigureAwait(false);
            res.StatusCode = 500;
        }
    }
}