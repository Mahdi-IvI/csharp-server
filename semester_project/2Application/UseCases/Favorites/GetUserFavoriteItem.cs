namespace semester_project._2Application.UseCases.Favorites;

public record GetUserFavoriteItem(
    long MediaId,
    string Title,
    string MediaType,
    string? ReleaseYear,
    short AgeRestriction,
    DateTime FavoritedAt
);