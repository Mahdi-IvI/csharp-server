namespace semester_project._1Presentation.Http.Contracts.Recommendations;

public record RecommendationItemResponse(
    long MediaId,
    string Title,
    string MediaType,
    string? ReleaseYear,
    short AgeRestriction,
    List<string> Genres,
    double AverageStars
);

public record UserRecommendationsResponse(List<RecommendationItemResponse> Items);