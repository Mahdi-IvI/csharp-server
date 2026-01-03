using semester_project._2Application.Interfaces;
using semester_project._3Domain.Entities;

namespace semester_project._2Application.UseCases.Ratings;

public class CreateRatingHandler(IRatingRepository ratings, IMediaRepository mediaRepo)
{
    public async Task<CreateRatingResult> HandleAsync(CreateRatingInput input)
    {
        if (input.stars < 1 || input.stars > 5)
            throw new ArgumentException("stars must be between 1 and 5.");

        // Ensure media exists (otherwise FK violation; explicit check gives clean 404)
        var creator = await mediaRepo.GetCreatorIdAsync(input.mediaId).ConfigureAwait(false);
        if (creator is null)
            throw new KeyNotFoundException("Media not found.");

        var rating = new Rate(
            0,
            input.userId,
            input.mediaId,
            input.stars,
            input.comment,
            DateTime.Now,
            false
        );

        var id = await ratings.AddAsync(rating).ConfigureAwait(false);
        return new CreateRatingResult(id);
    }
}