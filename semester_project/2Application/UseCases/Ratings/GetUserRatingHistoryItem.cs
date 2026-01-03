namespace semester_project._2Application.UseCases.Ratings;

public record GetUserRatingHistoryItem(
    long RatingId,
    long MediaId,
    string MediaTitle,
    short Stars,
    string? Comment,
    bool IsCommentConfirmed,
    DateTime CreatedAt
);