namespace semester_project._1Presentation.Http.Contracts.Users;

public record UpdateUserProfileRequest(
    string? Email,
    string? FirstName,
    string? LastName,
    string? FavoriteGenre
);