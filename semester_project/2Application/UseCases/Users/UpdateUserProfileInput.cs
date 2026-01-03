namespace semester_project._2Application.UseCases.Users;

public record UpdateUserProfileInput(
    long UserId,
    string? Email,
    string? FirstName,
    string? LastName,
    string? FavoriteGenre);