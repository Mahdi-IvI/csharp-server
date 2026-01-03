namespace semester_project._2Application.UseCases.Ratings;

public record UpdateRatingInput(long ratingId, long requesterUserId, short stars, string? comment);