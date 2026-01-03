namespace semester_project._1Presentation.Http.Contracts.Users;

public record UserProfileResponse(
    long Id,
    string Username,
    string? FirstName,
    string? LastName,
    string? FavoriteGenre,
    DateTime CreatedAt
);