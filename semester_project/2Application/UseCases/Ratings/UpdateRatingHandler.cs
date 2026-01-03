using semester_project._2Application.Interfaces;

namespace semester_project._2Application.UseCases.Ratings;

public class UpdateRatingHandler(IRatingRepository ratings)
{
    public async Task HandleAsync(UpdateRatingInput input)
    {
        if (input.stars < 1 || input.stars > 5)
            throw new ArgumentException("stars must be between 1 and 5.");

        var owner = await ratings.GetOwnerAndMediaAsync(input.ratingId).ConfigureAwait(false);
        if (owner is null)
            throw new KeyNotFoundException("Rating not found.");

        if (owner.Value.userId != input.requesterUserId)
            throw new InvalidOperationException("Forbidden.");

        var ok = await ratings.UpdateAsync(
            ratingId: input.ratingId,
            stars: input.stars,
            comment: input.comment
        ).ConfigureAwait(false);

        if (!ok) throw new KeyNotFoundException("Rating not found.");
    }
}