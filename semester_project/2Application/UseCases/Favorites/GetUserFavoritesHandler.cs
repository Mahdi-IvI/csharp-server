using semester_project._2Application.Interfaces;

namespace semester_project._2Application.UseCases.Favorites;

public class GetUserFavoritesHandler(IFavoriteRepository favorites)
{
    public Task<IReadOnlyList<GetUserFavoriteItem>> HandleAsync(GetUserFavoritesInput input)
    {
        return favorites.ListByUserAsync(input.UserId);
    }
}