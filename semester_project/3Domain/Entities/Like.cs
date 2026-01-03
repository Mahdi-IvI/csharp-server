namespace semester_project._3Domain.Entities;

public record Like(
    long Id,
    long UserId,
    long RatingId,
    DateTime CreatedAt);