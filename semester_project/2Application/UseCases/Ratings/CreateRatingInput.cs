namespace semester_project._2Application.UseCases.Ratings;

public record CreateRatingInput(
    long mediaId,
    long userId,
    short stars,
    string? comment
);