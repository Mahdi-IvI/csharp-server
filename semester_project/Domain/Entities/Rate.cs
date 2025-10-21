namespace semester_project.models;

public class Rate(
    string id,
    string userId,
    string mediaId,
    int star,
    string? comment,
    string timestamp,
    bool isConfirmed)
{
    public string Id { get; set; } = id;
    public string UserId { get; set; } = userId;
    public string MediaId { get; set; } = mediaId;
    public int Star { get; set; } = star;
    public string? Comment { get; set; } = comment;
    public string Timestamp { get; set; } = timestamp;
    public bool IsConfirmed { get; set; } = isConfirmed;
}