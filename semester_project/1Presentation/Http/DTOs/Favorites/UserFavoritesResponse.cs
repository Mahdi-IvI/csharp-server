namespace semester_project._1Presentation.Http.Contracts.Favorites;

public record UserFavoriteItemResponse(
    long MediaId,
    string Title,
    string MediaType,
    string? ReleaseYear,
    short AgeRestriction,
    DateTime FavoritedAt
);

public record UserFavoritesResponse(List<UserFavoriteItemResponse> Items);