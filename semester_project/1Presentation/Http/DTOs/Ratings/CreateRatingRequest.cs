namespace semester_project._1Presentation.Http.Contracts.Ratings;

public record CreateRatingRequest(
    short Stars,
    string? Comment
);