namespace semester_project.Presentation.Http.Contracts.Users;

public class UserProfileResponse
{
    public long Id { get; init; }
    public string Username { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? FavoriteGenre { get; init; }   // <--- NEW (string form)
    public DateTime CreatedAt { get; init; }
}