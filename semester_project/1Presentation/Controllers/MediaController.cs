using System.Text.Json;
using semester_project._2Application.UseCases.Media;
using semester_project._1Presentation.Http;
using semester_project._1Presentation.Http.Contracts;
using semester_project._1Presentation.Http.Contracts.Media;
using semester_project._1Presentation.Http.Contracts.Ratings;
using semester_project._1Presentation.Http.Routing.Attributes;
using semester_project._2Application.UseCases.Favorites;
using semester_project._2Application.UseCases.Ratings;

namespace semester_project._1Presentation.Controllers;

[Route("media")]
public sealed class MediaController
{
    private static readonly JsonSerializerOptions Json =
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    private static bool TryGetBearerToken(HttpRequest req, out string token)
    {
        token = string.Empty;
        var hdr = req.GetHeader("Authorization");
        if (hdr is null || !hdr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return false;
        token = hdr.Substring("Bearer ".Length).Trim();
        return token.Length > 0;
    }


    // POST /media/
    [HttpPost]
    public async Task Create(HttpRequest req, HttpResponse res)
    {
        // 1) Auth
        if (!TryGetBearerToken(req, out var bearer) || !App.Tokens.TryGetUsername(bearer, out var username))
        {
            res.StatusCode = 401;
            return;
        }

        try
        {
            // find the user by username
            var user = await App.GetUserByUsername.HandleAsync(username).ConfigureAwait(false);

            // parse body
            var body = await req.ReadBodyAsStringAsync().ConfigureAwait(false);
            var dto = JsonSerializer.Deserialize<UpsertMediaRequest>(body, Json);

            if (dto is null)
            {
                res.StatusCode = 400;
                return;
            }

            var genres = dto.Genres ?? new List<string>();

            var result = await App.CreateMedia.HandleAsync(new CreateMediaInput(
                user.Id,
                dto.Title ?? string.Empty, dto.Description,
                dto.MediaType ?? string.Empty,
                dto.ReleaseYear,
                genres,
                dto.AgeRestriction
            )).ConfigureAwait(false);

            // 201 Created
            await res.WriteJsonAsync(new CreateMediaResponse(result.Id), statusCode: 201).ConfigureAwait(false);
            res.SetHeader("Location", $"/api/media/{result.Id}");
        }
        catch (KeyNotFoundException)
        {
            // token user not found (shouldn't happen normally)
            res.StatusCode = 401;
        }
        catch (ArgumentException e)
        {
            await res.WriteJsonAsync(new ApiError("BadRequest", e.Message), 400);
            res.StatusCode = 400;
        }
        catch (Exception)
        {
            res.StatusCode = 500;
        }
    }

    // DELETE /media/{id}
    [HttpDelete("{id}")]
    public async Task Delete(HttpRequest req, HttpResponse res)
    {
        // Auth
        if (!TryGetBearerToken(req, out var bearer) || !App.Tokens.TryGetUsername(bearer, out var username))
        {
            res.StatusCode = 401;
            return;
        }

        // Path id
        if (!req.TryGetRouteValue("id", out var idRaw) || !long.TryParse(idRaw, out var mediaId))
        {
            res.StatusCode = 400;
            return;
        }

        try
        {
            var user = await App.GetUserByUsername.HandleAsync(username).ConfigureAwait(false);

            await App.DeleteMedia.HandleAsync(new DeleteMediaInput(
                mediaId,
                user.Id
            )).ConfigureAwait(false);

            res.StatusCode = 204; // No Content
        }
        catch (KeyNotFoundException)
        {
            await res.WriteJsonAsync(new ApiError("NotFound", "Media Not Found"), 404);
            res.StatusCode = 404;
        }
        catch (InvalidOperationException)
        {
            res.StatusCode = 403;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            res.StatusCode = 500;
        }
    }

    // PUT /media/{id}
    [HttpPut("{id}")]
    public async Task Update(HttpRequest req, HttpResponse res)
    {
        // Auth
        if (!TryGetBearerToken(req, out var bearer) || !App.Tokens.TryGetUsername(bearer, out var username))
        {
            res.StatusCode = 401;
            return;
        }

        // Path id
        if (!req.TryGetRouteValue("id", out var idRaw) || !long.TryParse(idRaw, out var mediaId))
        {
            res.StatusCode = 400;
            return;
        }

        // parse body
        var body = await req.ReadBodyAsStringAsync().ConfigureAwait(false);
        var dto = JsonSerializer.Deserialize<UpsertMediaRequest>(body, Json);

        if (dto is null)
        {
            res.StatusCode = 400;
            return;
        }

        var genres = dto.Genres ?? new List<string>();

        try
        {
            var user = await App.GetUserByUsername.HandleAsync(username).ConfigureAwait(false);

            await App.UpdateMedia.HandleAsync(new UpdateMediaInput(
                user.Id,
                mediaId,
                dto.Title ?? string.Empty,
                dto.Description,
                dto.MediaType ?? string.Empty,
                dto.ReleaseYear,
                genres,
                dto.AgeRestriction
            )).ConfigureAwait(false);

            res.StatusCode = 204; // No Content
        }
        catch (KeyNotFoundException)
        {
            await res.WriteJsonAsync(new ApiError("NotFound", "Media Not Found"), 404);
            res.StatusCode = 404;
        }
        catch (InvalidOperationException)
        {
            res.StatusCode = 403;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            res.StatusCode = 500;
        }
    }

    // GET /media/{id}
    [HttpGet("{id}")]
    public async Task GetById(HttpRequest req, HttpResponse res)
    {
        // Auth (required for all non-login endpoints)  [oai_citation:9‡MRP_Specification.pdf](file-service://file-Gsrd96Edmkjcy3vPBg9XCM)
        if (!TryGetBearerToken(req, out var bearer) || !App.Tokens.TryGetUsername(bearer, out _))
        {
            res.StatusCode = 401;
            return;
        }

        // Path id
        if (!req.TryGetRouteValue("id", out var idRaw) || !long.TryParse(idRaw, out var mediaId))
        {
            res.StatusCode = 400;
            return;
        }

        try
        {
            var dto = await App.GetMediaById
                .HandleAsync(new GetMediaByIdInput(mediaId))
                .ConfigureAwait(false);

            // 200 OK
            await res.WriteJsonAsync(dto, statusCode: 200).ConfigureAwait(false);
        }
        catch (KeyNotFoundException)
        {
            await res.WriteJsonAsync(new ApiError("NotFound", "Media Not Found"), 404);
            res.StatusCode = 404;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            res.StatusCode = 500;
        }
    }


    // POST /media/{id}/rate
    [HttpPost("{id}/rate")]
    public async Task CreateForMedia(HttpRequest req, HttpResponse res)
    {
        if (!TryGetBearerToken(req, out var bearer) || !App.Tokens.TryGetUsername(bearer, out var username))
        {
            res.StatusCode = 401;
            return;
        }

        if (!req.TryGetRouteValue("id", out var idRaw) || !long.TryParse(idRaw, out var mediaId))
        {
            res.StatusCode = 400;
            return;
        }

        var body = await req.ReadBodyAsStringAsync().ConfigureAwait(false);
        var dto = JsonSerializer.Deserialize<CreateRatingRequest>(body, Json);
        if (dto is null)
        {
            res.StatusCode = 400;
            return;
        }

        try
        {
            var user = await App.GetUserByUsername.HandleAsync(username).ConfigureAwait(false);

            var result = await App.CreateRate.HandleAsync(new CreateRatingInput(
                mediaId: mediaId,
                userId: user.Id,
                stars: dto.Stars,
                comment: dto.Comment
            )).ConfigureAwait(false);

            res.SetHeader("Location", $"/api/ratings/{result.id}");
            res.StatusCode = 201;
        }
        catch (KeyNotFoundException)
        {
            await res.WriteJsonAsync(new ApiError("NotFound", "Media Not Found"), 404);
        }
        catch (ArgumentException e)
        {
            await res.WriteJsonAsync(new ApiError("BadRequest", e.Message), 400);
        }
        catch
        {
            res.StatusCode = 500;
        }
    }

    // POST /media/{id}/favorite
    [HttpPost("{id}/favorite")]
    public async Task MarkMediaAsFavorite(HttpRequest req, HttpResponse res)
    {
        if (!TryGetBearerToken(req, out var bearer) || !App.Tokens.TryGetUsername(bearer, out var username))
        {
            res.StatusCode = 401;
            return;
        }

        if (!req.TryGetRouteValue("id", out var idRaw) || !long.TryParse(idRaw, out var mediaId))
        {
            res.StatusCode = 400;
            return;
        }

        try
        {
            var user = await App.GetUserByUsername.HandleAsync(username).ConfigureAwait(false);

            await App.FavoriteMedia.HandleAsync(new FavoriteInput(
                mediaId: mediaId,
                userId: user.Id
            )).ConfigureAwait(false);

            res.StatusCode = 201;
        }
        catch (KeyNotFoundException)
        {
            await res.WriteJsonAsync(new ApiError("NotFound", "Media Not Found"), 404);
        }
        catch
        {
            res.StatusCode = 500;
        }
    }

    // DELETE /media/{id}/favorite
    [HttpDelete("{id}/favorite")]
    public async Task UnmarkMediaAsFavorite(HttpRequest req, HttpResponse res)
    {
        if (!TryGetBearerToken(req, out var bearer) || !App.Tokens.TryGetUsername(bearer, out var username))
        {
            res.StatusCode = 401;
            return;
        }

        if (!req.TryGetRouteValue("id", out var idRaw) || !long.TryParse(idRaw, out var mediaId))
        {
            res.StatusCode = 400;
            return;
        }

        try
        {
            var user = await App.GetUserByUsername.HandleAsync(username).ConfigureAwait(false);

            await App.UnfavoriteMedia.HandleAsync(new FavoriteInput(
                mediaId: mediaId,
                userId: user.Id
            )).ConfigureAwait(false);

            res.StatusCode = 201;
        }
        catch (KeyNotFoundException)
        {
            await res.WriteJsonAsync(new ApiError("NotFound", "Media Not Found"), 404);
        }
        catch
        {
            res.StatusCode = 500;
        }
    }


    // GET /media?title=incep&genre=sci-fi&mediaType=movie&releaseYear=2010&ageRestriction=12&rating=4&sortBy=title
    [HttpGet]
    public async Task Search(HttpRequest req, HttpResponse res)
    {
        // Auth (spec: all endpoints except register/login)  [oai_citation:3‡MRP_Specification.pdf](file-service://file-Gsrd96Edmkjcy3vPBg9XCM)
        if (!TryGetBearerToken(req, out var bearer) || !App.Tokens.TryGetUsername(bearer, out _))
        {
            res.StatusCode = 401;
            return;
        }

        string? title = req.Query["title"];
        string? genre = req.Query["genre"];
        string? mediaType = req.Query["mediaType"];
        string? sortBy = req.Query["sortBy"];

        int? releaseYear = null;
        var ryRaw = req.Query["releaseYear"];
        if (!string.IsNullOrWhiteSpace(ryRaw))
        {
            if (!int.TryParse(ryRaw, out var parsed))
            {
                await res.WriteJsonAsync(new ApiError("BadRequest", "releaseYear must be a number."), 400)
                    .ConfigureAwait(false);
                res.StatusCode = 400;
                return;
            }

            releaseYear = parsed;
        }

        short? ageRestriction = null;
        var arRaw = req.Query["ageRestriction"];
        if (!string.IsNullOrWhiteSpace(arRaw))
        {
            if (!short.TryParse(arRaw, out var parsed))
            {
                await res.WriteJsonAsync(new ApiError("BadRequest", "ageRestriction must be a number."), 400)
                    .ConfigureAwait(false);
                res.StatusCode = 400;
                return;
            }

            ageRestriction = parsed;
        }

        double? rating = null;
        var ratingRaw = req.Query["rating"];
        if (!string.IsNullOrWhiteSpace(ratingRaw))
        {
            if (!double.TryParse(ratingRaw, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            {
                await res.WriteJsonAsync(new ApiError("BadRequest", "rating must be a number."), 400)
                    .ConfigureAwait(false);
                res.StatusCode = 400;
                return;
            }

            rating = parsed;
        }

        try
        {
            var items = await App.SearchMedia.HandleAsync(new SearchMediaInput(
                title,
                genre,
                mediaType,
                releaseYear,
                ageRestriction,
                rating,
                sortBy
            )).ConfigureAwait(false);

            var response = new SearchMediaResponse(
                items.Select(x => new MediaListItemResponse(
                    Id: x.Id,
                    Title: x.Title,
                    Description: x.Description,
                    MediaType: x.MediaType,
                    ReleaseYear: x.ReleaseYear,
                    AgeRestriction: x.AgeRestriction,
                    Genres: x.Genres,
                    Score: x.Score
                )).ToList()
            );

            await res.WriteJsonAsync(response).ConfigureAwait(false);
        }
        catch (ArgumentException e)
        {
            await res.WriteJsonAsync(new ApiError("BadRequest", e.Message), 400).ConfigureAwait(false);
            res.StatusCode = 400;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            res.StatusCode = 500;
        }
    }
}