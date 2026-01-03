namespace semester_project._2Application.UseCases.Recommendations;

public record GetUserRecommendationsInput(
    long UserId,
    RecommendationType Type,
    int Limit
);