using System.Text.Json;
using semester_project.Application.UseCases.Media;
using semester_project.Presentation.Http;
using semester_project.Presentation.Http.Contracts;
using semester_project.Presentation.Http.Contracts.Media;
using semester_project.Presentation.Http.Routing.Attributes;

namespace semester_project.Presentation.Controllers;

[Route("media")]
public sealed class MediaController
{
    private static readonly JsonSerializerOptions _json =
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
            var dto = JsonSerializer.Deserialize<CreateMediaRequest>(body, _json);

            if (dto is null)
            {
                res.StatusCode = 400;
                return;
            }

            var genres = dto.Genres ?? new System.Collections.Generic.List<string>();

            var result = await App.CreateMedia.HandleAsync(new CreateMediaInput(
                createdByUserId: user.Id,
                title: dto.Title ?? string.Empty,
                description: dto.Description,
                mediaType: dto.MediaType ?? string.Empty,
                releaseYear: dto.ReleaseYear,
                genres: genres,
                ageRestriction: dto.AgeRestriction
            )).ConfigureAwait(false);

            // 201 Created
            await res.WriteJsonAsync(new CreateMediaResponse(result.Id), statusCode: 201).ConfigureAwait(false);
            res.SetHeader("Location", $"/api/media/{result.Id}");
        }
        catch (System.Collections.Generic.KeyNotFoundException)
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
                mediaId: mediaId,
                requesterUserId: user.Id
            )).ConfigureAwait(false);

            res.StatusCode = 204; // No Content
        }
        catch (System.Collections.Generic.KeyNotFoundException)
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
    public Task Update(HttpRequest req, HttpResponse res)
    {
        return Task.CompletedTask;
    }

    // GET /media/{id}
    [HttpGet("{id}")]
    public Task GetById(HttpRequest req, HttpResponse res)
    {
        return Task.CompletedTask;
    }
}