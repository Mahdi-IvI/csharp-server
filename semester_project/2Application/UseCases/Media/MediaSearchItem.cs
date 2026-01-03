namespace semester_project._2Application.UseCases.Media;

public record MediaSearchItem(
    long Id,
    string Title,
    string? Description,
    string MediaType,
    string? ReleaseYear,
    short AgeRestriction,
    List<string> Genres,
    double Score
);