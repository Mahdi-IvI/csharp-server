namespace semester_project.Presentation.Http.Contracts.Media;

public class CreateMediaResponse
{
    public CreateMediaResponse(long id) { Id = id; }
    public long Id { get; }
}