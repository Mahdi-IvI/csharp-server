namespace semester_project._1Presentation.Http.Contracts.Leaderboard;

public record LeaderboardEntryResponse(
    long UserId,
    string Username,
    int TotalRatings,
    double AverageStars
);

public record LeaderboardResponse(List<LeaderboardEntryResponse> Items);