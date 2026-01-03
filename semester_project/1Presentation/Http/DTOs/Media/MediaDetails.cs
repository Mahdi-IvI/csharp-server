namespace semester_project._1Presentation.Http.Contracts.Media;

public record MediaDetails(
    long Id,
    string Title,
    string? Description,
    string MediaType,
    string? ReleaseYear,
    short AgeRestriction,
    long CreatedBy,
    DateTime CreatedAt,
    IReadOnlyList<string> Genres,
    double? AverageScore,
    int RatingCount
);