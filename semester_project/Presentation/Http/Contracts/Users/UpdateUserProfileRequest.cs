namespace semester_project.Presentation.Http.Contracts.Users;

public class UpdateUserProfileRequest
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FavoriteGenre { get; set; } 
}