using semester_project._2Application.UseCases.Favorites;

namespace semester_project._2Application.Interfaces;

public interface IFavoriteRepository
{
    Task<IReadOnlyList<GetUserFavoriteItem>> ListByUserAsync(long userId);
}