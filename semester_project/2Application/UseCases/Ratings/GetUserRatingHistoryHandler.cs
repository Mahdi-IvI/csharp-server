using semester_project._2Application.Interfaces;

namespace semester_project._2Application.UseCases.Ratings;

public class GetUserRatingHistoryHandler(IRatingRepository ratings)
{
    public Task<IReadOnlyList<GetUserRatingHistoryItem>> HandleAsync(GetUserRatingHistoryInput input)
    {
        return ratings.ListByUserAsync(input.UserId);
    }
}