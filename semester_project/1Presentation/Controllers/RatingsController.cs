using semester_project._1Presentation.Http;
using semester_project._1Presentation.Http.Contracts;
using semester_project._1Presentation.Http.Contracts.Ratings;
using semester_project._1Presentation.Http.Routing.Attributes;
using semester_project._2Application.UseCases.Ratings;

namespace semester_project._1Presentation.Controllers;

using System.Text.Json;

[Route("ratings")]
public sealed class RatingsController
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

    // PUT /ratings/{id}
    [HttpPut("{id}")]
    public async Task Update(HttpRequest req, HttpResponse res)
    {
        if (!TryGetBearerToken(req, out var bearer) || !App.Tokens.TryGetUsername(bearer, out var username))
        {
            res.StatusCode = 401;
            return;
        }

        if (!req.TryGetRouteValue("id", out var idRaw) || !long.TryParse(idRaw, out var ratingId))
        {
            res.StatusCode = 400;
            return;
        }

        var body = await req.ReadBodyAsStringAsync().ConfigureAwait(false);
        var dto = JsonSerializer.Deserialize<UpdateRatingRequest>(body, Json);
        if (dto is null)
        {
            res.StatusCode = 400;
            return;
        }

        try
        {
            var user = await App.GetUserByUsername.HandleAsync(username).ConfigureAwait(false);

            await App.UpdateRate.HandleAsync(new UpdateRatingInput(
                ratingId: ratingId,
                requesterUserId: user.Id,
                stars: dto.Stars,
                comment: dto.Comment
            )).ConfigureAwait(false);

            res.StatusCode = 204;
        }
        catch (KeyNotFoundException)
        {
            await res.WriteJsonAsync(new ApiError("NotFound", "Rating Not Found"), 404);
        }
        catch (InvalidOperationException)
        {
            res.StatusCode = 403;
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

    // DELETE /ratings/{id}
    [HttpDelete("{id}")]
    public async Task Delete(HttpRequest req, HttpResponse res)
    {
        if (!TryGetBearerToken(req, out var bearer) || !App.Tokens.TryGetUsername(bearer, out var username))
        {
            res.StatusCode = 401;
            return;
        }

        if (!req.TryGetRouteValue("id", out var idRaw) || !long.TryParse(idRaw, out var ratingId))
        {
            res.StatusCode = 400;
            return;
        }

        try
        {
            var user = await App.GetUserByUsername.HandleAsync(username).ConfigureAwait(false);

            await App.DeleteRate.HandleAsync(new DeleteRatingInput(ratingId, user.Id)).ConfigureAwait(false);
            res.StatusCode = 204;
        }
        catch (KeyNotFoundException)
        {
            await res.WriteJsonAsync(new ApiError("NotFound", "Rating Not Found"), 404);
        }
        catch (InvalidOperationException)
        {
            res.StatusCode = 403;
        }
        catch
        {
            res.StatusCode = 500;
        }
    }

    // POST /ratings/{id}/confirm   (admin only)
    [HttpPost("{id}/confirm")]
    public async Task Approve(HttpRequest req, HttpResponse res)
    {
        Console.WriteLine("try to confirm rating");
        if (!TryGetBearerToken(req, out var bearer) || !App.Tokens.TryGetUsername(bearer, out var username))
        {
            res.StatusCode = 401;
            return;
        }

        if (!req.TryGetRouteValue("id", out var idRaw) || !long.TryParse(idRaw, out var ratingId))
        {
            res.StatusCode = 400;
            return;
        }

        try
        {
            await App.ConfirmRate.HandleAsync(new ApproveRatingCommentInput(ratingId))
                .ConfigureAwait(false);

            res.StatusCode = 204;
        }
        catch (KeyNotFoundException)
        {
            await res.WriteJsonAsync(new ApiError("NotFound", "Rating Not Found"), 404);
        }
        catch
        {
            res.StatusCode = 500;
        }
    }

    // POST /ratings/{id}/confirm   (admin only)
    [HttpPost("{id}/like")]
    public async Task LikeRating(HttpRequest req, HttpResponse res)
    {
        if (!TryGetBearerToken(req, out var bearer) || !App.Tokens.TryGetUsername(bearer, out var username))
        {
            res.StatusCode = 401;
            return;
        }

        var user = await App.GetUserByUsername.HandleAsync(username).ConfigureAwait(false);


        if (!req.TryGetRouteValue("id", out var idRaw) || !long.TryParse(idRaw, out var ratingId))
        {
            res.StatusCode = 400;
            return;
        }

        try
        {
            await App.LikeRate.HandleAsync(new LikeRateInput(ratingId, user.Id))
                .ConfigureAwait(false);

            res.StatusCode = 204;
        }
        catch (KeyNotFoundException)
        {
            await res.WriteJsonAsync(new ApiError("NotFound", "Rating not found."), 404);
        }
        catch (DuplicateWaitObjectException)
        {
            await res.WriteJsonAsync(new ApiError("Conflict", "The rating is already liked."), 409);
        }
        catch
        {
            res.StatusCode = 500;
        }
    }
}