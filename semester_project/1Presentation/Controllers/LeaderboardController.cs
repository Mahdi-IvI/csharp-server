using semester_project._1Presentation.Http;
using semester_project._1Presentation.Http.Contracts;
using semester_project._1Presentation.Http.Contracts.Leaderboard;
using semester_project._1Presentation.Http.Routing.Attributes;

namespace semester_project._1Presentation.Controllers;

[Route("leaderboard")]
public sealed class LeaderboardController
{
    // GET /leaderboard?limit=10
    [HttpGet]
    public async Task Get(HttpRequest req, HttpResponse res)
    {
        try
        {
            var items = await App.GetLeaderboard.HandleAsync()
                .ConfigureAwait(false);

            var response = new LeaderboardResponse(
                items.Select(x => new LeaderboardEntryResponse(
                    UserId: x.UserId,
                    Username: x.Username,
                    TotalRatings: x.TotalRatings,
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
        catch (Exception e)
        {
            Console.WriteLine(e);
            await res.WriteJsonAsync(new ApiError("ServerError", "Unexpected error"), 500).ConfigureAwait(false);
            res.StatusCode = 500;
        }
    }
}