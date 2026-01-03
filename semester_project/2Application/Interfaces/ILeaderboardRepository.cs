using semester_project._2Application.UseCases.Leaderboard;

namespace semester_project._2Application.Interfaces;

public interface ILeaderboardRepository
{
    Task<IReadOnlyList<LeaderboardEntry>> GetLeaderboardAsync();
}