
namespace semester_project.Domain.Entities;

public class Favorite(string id, string userId, string mediaId)
{
    public string Id { get; set; } = id;
    public string UserId { get; set; } = userId;
    public string MediaId { get; set; } = mediaId;
}