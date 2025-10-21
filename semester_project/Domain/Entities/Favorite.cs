namespace semester_project.models;

public class Favorite(string id, string userId, string mediaId)
{
    public string Id { get; set; } = id;
    public string UserId { get; set; } = userId;
    public string MediaId { get; set; } = mediaId;
}