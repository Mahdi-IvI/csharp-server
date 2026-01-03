using semester_project._3Domain.Enums;

namespace semester_project._3Domain.Entities;

public record User(
    long Id,
    string Username,
    string Password,
    string? FirstName,
    string? LastName,
    Genre? FavoriteGenre,
    DateTime CreatedAt);