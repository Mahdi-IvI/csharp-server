namespace semester_project._2Application.UseCases.Media;

public record SearchMediaInput(
    string? Title,
    string? Genre,
    string? MediaType,
    int? ReleaseYear,
    short? AgeRestriction,
    double? Rating,
    string? SortBy);