namespace semester_project._1Presentation.Http.Contracts.Media;

public record MediaListItemResponse(
    long Id,
    string Title,
    string? Description,
    string MediaType,
    string? ReleaseYear,
    short AgeRestriction,
    List<string> Genres,
    double Score
);

public record SearchMediaResponse(List<MediaListItemResponse> Items);