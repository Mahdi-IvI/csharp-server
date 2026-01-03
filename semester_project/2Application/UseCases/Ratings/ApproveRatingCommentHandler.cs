using semester_project._2Application.Interfaces;

namespace semester_project._2Application.UseCases.Ratings;

public class ApproveRatingCommentHandler(IRatingRepository ratings)
{
    public async Task HandleAsync(ApproveRatingCommentInput input)
    {
        var ok = await ratings.SetCommentConfirmedAsync(input.ratingId).ConfigureAwait(false);
        if (!ok) throw new KeyNotFoundException("Rating not found.");
    }
}