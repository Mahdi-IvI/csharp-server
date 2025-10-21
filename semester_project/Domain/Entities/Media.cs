
namespace semester_project.Domain.Entities;

public enum MediaType
{
    movie,
    series,
    game
}

public enum Genres
{
    movie,
    series,
    game
}

public enum AgeRestriction
{
    example,
    example2
}

public class Media(
    string id,
    string title,
    string description,
    MediaType type,
    string releaseYear,
    List<Genres> genres,
    AgeRestriction ageRestriction,
    string creator,
    int averageScore)
{
    public string Id { get; set; } = id;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public MediaType Type { get; set; } = type;
    public string ReleaseYear { get; set; } = releaseYear;
    public List<Genres> Genres { get; set; } = genres;
    public AgeRestriction AgeRestriction { get; set; } = ageRestriction;
    public string Creator { get; set; } = creator;
    public int AverageScore { get; set; } = averageScore;
}