using semester_project._2Application.Interfaces;

namespace semester_project._2Application.UseCases.Leaderboard;

public class GetLeaderboardHandler(ILeaderboardRepository repo)
{
    public Task<IReadOnlyList<LeaderboardEntry>> HandleAsync()
    {
        return repo.GetLeaderboardAsync();
    }
}