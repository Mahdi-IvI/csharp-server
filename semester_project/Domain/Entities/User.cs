using semester_project.Domain.Enums;

namespace semester_project.Domain.Entities;

public class User(
    long id,
    string username,
    string password,
    string? firstName,
    string? lastName,
    Genre? favoriteGenre,
    DateTime createdAt)
{
    public long Id { get; set; } = id;
    public string Username { get; set; } = username;
    public string Password { get; set; } = password;
    public string? FirstName { get; set; } = firstName;
    public string? LastName { get; set; } = lastName;
    public Genre? FavoriteGenre { get; set; } = favoriteGenre;
    public DateTime CreatedAt { get; set; } = createdAt;
}