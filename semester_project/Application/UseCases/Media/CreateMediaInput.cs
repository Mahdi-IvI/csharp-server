namespace semester_project.Application.UseCases.Media;

public class CreateMediaInput
{
    public CreateMediaInput(
        long createdByUserId,
        string title,
        string? description,
        string mediaType,
        int? releaseYear,
        IEnumerable<string> genres,
        short ageRestriction)
    {
        CreatedByUserId = createdByUserId;
        Title = title;
        Description = description;
        MediaType = mediaType;
        ReleaseYear = releaseYear;
        Genres = genres;
        AgeRestriction = ageRestriction;
    }

    public long CreatedByUserId { get; }
    public string Title { get; }
    public string? Description { get; }
    public string MediaType { get; }
    public int? ReleaseYear { get; }
    public IEnumerable<string> Genres { get; }
    public short AgeRestriction { get; }
}