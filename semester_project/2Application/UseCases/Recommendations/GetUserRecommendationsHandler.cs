using semester_project._2Application.Interfaces;

namespace semester_project._2Application.UseCases.Recommendations;

public class GetUserRecommendationsHandler(IRecommendationRepository repo)
{
    public async Task<IReadOnlyList<RecommendationItem>> HandleAsync(GetUserRecommendationsInput input)
    {
        if (input.Limit <= 0 || input.Limit > 50)
            throw new ArgumentException("limit must be between 1 and 50.");

        return input.Type switch
        {
            RecommendationType.Genre => await repo.RecommendByGenreAsync(input.UserId, input.Limit)
                .ConfigureAwait(false),
            RecommendationType.Content => await repo.RecommendByContentAsync(input.UserId, input.Limit)
                .ConfigureAwait(false),
            _ => throw new ArgumentException("Invalid recommendation type.")
        };
    }
}