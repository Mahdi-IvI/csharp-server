using semester_project._3Domain.Enums;

namespace semester_project._3Domain.Entities;

public record Media(
    long Id,
    string Title,
    string Description,
    MediaType Type,
    string ReleaseYear,
    List<Genre> Genres,
    int AgeRestriction,
    long CreatedBy,
    DateTime CreatedOn
);