using semester_project._2Application.Interfaces;
using semester_project._3Domain.Entities;

namespace semester_project._2Application.UseCases.Favorites;

public class UnfavoriteMediaHandler(IMediaRepository mediaRepo)
{
    public async Task<bool> HandleAsync(FavoriteInput input)
    {
        // Ensure media exists (otherwise FK violation; explicit check gives clean 404)
        var creator = await mediaRepo.GetCreatorIdAsync(input.mediaId).ConfigureAwait(false);
        if (creator is null)
            throw new KeyNotFoundException("Media not found.");

        var rating = new Favorite(
            input.userId,
            input.mediaId,
            DateTime.Now
        );

        var id = await mediaRepo.UnfavoriteAsync(rating).ConfigureAwait(false);
        return id;
    }
}