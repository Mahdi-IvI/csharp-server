namespace semester_project.Presentation.Http.Contracts.Media;

public class CreateMediaRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? MediaType { get; set; }      // "movie" | "series" | "game"
    public int? ReleaseYear { get; set; }
    public List<string>? Genres { get; set; }   // e.g., ["sci-fi","thriller"]
    public short AgeRestriction { get; set; }   // 0,6,12,16,18
}