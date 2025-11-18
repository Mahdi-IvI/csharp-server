namespace semester_project.Application.UseCases.Users;

public class UpdateUserProfileInput(
    long userId,
    string? email,
    string? firstName,
    string? lastName,
    string? favoriteGenre)
{
    public long UserId { get; } = userId;
    public string? Email { get; } = email;
    public string? FirstName { get; } = firstName;
    public string? LastName { get; } = lastName;
    public string? FavoriteGenre { get; } = favoriteGenre;
}