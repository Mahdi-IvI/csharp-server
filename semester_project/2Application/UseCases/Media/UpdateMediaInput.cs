namespace semester_project._2Application.UseCases.Media;

public record UpdateMediaInput(
    long CreatedByUserId,
    long MediaId,
    string Title,
    string? Description,
    string MediaType,
    int? ReleaseYear,
    IEnumerable<string> Genres,
    short AgeRestriction);