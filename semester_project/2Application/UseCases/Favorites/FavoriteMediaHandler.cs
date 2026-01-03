using semester_project._2Application.Interfaces;
using semester_project._3Domain.Entities;

namespace semester_project._2Application.UseCases.Favorites;

public class FavoriteMediaHandler(IMediaRepository mediaRepo)
{
    public async Task<long> HandleAsync(FavoriteInput input)
    {
        // Ensure media exists (otherwise FK violation; explicit check gives clean 404)
        var creator = await mediaRepo.GetCreatorIdAsync(input.mediaId).ConfigureAwait(false);
        if (creator is null)
            throw new KeyNotFoundException("Media not found.");

        var favorite = await mediaRepo.GetFavoriteAsync(input.mediaId, input.userId).ConfigureAwait(false);
        if (favorite is not null)
            throw new DuplicateWaitObjectException("Media market already as favorite.");

        Console.WriteLine("try to  favorite media");

        var rating = new Favorite(
            input.userId,
            input.mediaId,
            DateTime.Now
        );

        var id = await mediaRepo.FavoriteAsync(rating).ConfigureAwait(false);
        return id;
    }
}