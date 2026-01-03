using semester_project._2Application.Interfaces;
using semester_project._3Domain.Entities;

namespace semester_project._2Application.UseCases.Ratings;

public class LikeRateHandler(IRatingRepository ratings)
{
    public async Task<CreateRatingResult> HandleAsync(LikeRateInput input)
    {
        // Ensure Rate exists
        var rate = await ratings.GetRateByIdAsync(input.ratingId).ConfigureAwait(false);
        if (rate is null)
            throw new KeyNotFoundException("Rating not found.");

        Console.WriteLine("Rating found");

        var likeRate = await ratings.GetLikeAsync(input.ratingId, input.userId).ConfigureAwait(false);
        if (likeRate is not null)
            throw new DuplicateWaitObjectException("The rating is already liked.");


        var like = new Like(
            0,
            input.userId,
            input.ratingId,
            DateTime.Now
        );

        var id = await ratings.LikeRateAsync(like).ConfigureAwait(false);

        Console.WriteLine(id);
        return new CreateRatingResult(id);
    }
}