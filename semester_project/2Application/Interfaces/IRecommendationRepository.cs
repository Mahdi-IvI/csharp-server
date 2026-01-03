using semester_project._2Application.UseCases.Recommendations;

namespace semester_project._2Application.Interfaces;

public interface IRecommendationRepository
{
    Task<IReadOnlyList<RecommendationItem>> RecommendByGenreAsync(long userId, int limit);
    Task<IReadOnlyList<RecommendationItem>> RecommendByContentAsync(long userId, int limit);
}