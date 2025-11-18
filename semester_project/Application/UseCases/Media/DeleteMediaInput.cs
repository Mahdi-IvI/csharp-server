namespace semester_project.Application.UseCases.Media;

public class DeleteMediaInput
{
    public DeleteMediaInput(long mediaId, long requesterUserId)
    {
        MediaId = mediaId;
        RequesterUserId = requesterUserId;
    }
    public long MediaId { get; }
    public long RequesterUserId { get; }
}