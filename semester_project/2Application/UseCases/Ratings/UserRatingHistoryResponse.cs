namespace semester_project._2Application.UseCases.Ratings;

public record UserRatingHistoryItemResponse(
    long RatingId,
    long MediaId,
    string MediaTitle,
    short Stars,
    string? Comment,
    bool IsCommentConfirmed,
    DateTime CreatedAt
);

public record UserRatingHistoryResponse(List<UserRatingHistoryItemResponse> Items);