namespace semester_project._3Domain.Entities;

public record Rate(
    long Id,
    long UserId,
    long MediaId,
    short Stars,
    string? Comment,
    DateTime CreatedAt,
    bool IsApproved);