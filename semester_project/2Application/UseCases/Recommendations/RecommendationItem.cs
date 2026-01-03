namespace semester_project._2Application.UseCases.Recommendations;

public record RecommendationItem(
    long MediaId,
    string Title,
    string MediaType,
    string? ReleaseYear,
    short AgeRestriction,
    List<string> Genres,
    double AverageStars
);