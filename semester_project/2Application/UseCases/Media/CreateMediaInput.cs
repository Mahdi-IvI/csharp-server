namespace semester_project._2Application.UseCases.Media;

public record CreateMediaInput(
    long CreatedByUserId,
    string Title,
    string? Description,
    string MediaType,
    int? ReleaseYear,
    IEnumerable<string> Genres,
    short AgeRestriction);