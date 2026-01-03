namespace semester_project._1Presentation.Http.Contracts.Ratings;

public record UpdateRatingRequest(
    short Stars,
    string? Comment
);