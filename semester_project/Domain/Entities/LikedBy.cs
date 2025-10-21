namespace semester_project.Domain.Entities;

public class LikedBy(string id, string userId, string rateId)
{
    public string Id { get; set; } = id;
    public string UserId { get; set; } = userId;
    public string RateId { get; set; } = rateId;
}