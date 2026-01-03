using semester_project._2Application.Interfaces;

namespace semester_project._2Application.UseCases.Ratings;

public class DeleteRatingHandler(IRatingRepository ratings)
{
    public async Task HandleAsync(DeleteRatingInput input)
    {
        var owner = await ratings.GetOwnerAndMediaAsync(input.ratingId).ConfigureAwait(false);
        if (owner is null)
            throw new KeyNotFoundException("Rating not found.");

        if (owner.Value.userId != input.requesterUserId)
            throw new InvalidOperationException("Forbidden.");

        var deleted = await ratings.DeleteAsync(input.ratingId).ConfigureAwait(false);
        if (!deleted) throw new KeyNotFoundException("Rating not found.");
    }
}