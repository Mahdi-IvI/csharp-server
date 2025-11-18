using semester_project.Domain.Enums;

namespace semester_project.Domain.Entities;

public class Media(
    long id,
    string title,
    string description,
    MediaType type,
    string releaseYear,
    List<Genre> genres,
    int ageRestriction,
    long createdBy,
    DateTime createdOn
    )
{
    public long Id { get; set; } = id;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public MediaType Type { get; set; } = type;
    public string ReleaseYear { get; set; } = releaseYear;
    public List<Genre> Genres { get; set; } = genres;
    public int AgeRestriction { get; set; } = ageRestriction;
    public long CreatedBy { get; set; } = createdBy;
    public DateTime CreatedOn { get; set; } = createdOn;

    public override string ToString()
    {
        return $"{Id} - {Title} - {Description}";
    }
}