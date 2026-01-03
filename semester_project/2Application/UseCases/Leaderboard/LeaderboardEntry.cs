namespace semester_project._2Application.UseCases.Leaderboard;

public record LeaderboardEntry(
    long UserId,
    string Username,
    int TotalRatings,
    double AverageStars
);