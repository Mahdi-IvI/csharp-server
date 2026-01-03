namespace semester_project._1Presentation.Http.Contracts.Media;

public record UpsertMediaRequest(
    string? Title,
    string? Description,
    string? MediaType,
    int? ReleaseYear,
    List<string>? Genres,
    short AgeRestriction
);