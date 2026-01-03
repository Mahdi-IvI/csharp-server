using semester_project._2Application.Interfaces;

namespace semester_project._2Application.UseCases.Media;

public class DeleteMediaHandler(IMediaRepository mediaRepo)
{
    private readonly IMediaRepository _mediaRepo = mediaRepo ?? throw new ArgumentNullException(nameof(mediaRepo));

    public async Task HandleAsync(DeleteMediaInput input)
    {
        var creatorId = await _mediaRepo.GetCreatorIdAsync(input.MediaId).ConfigureAwait(false);
        if (creatorId is null)
            throw new KeyNotFoundException("Media not found.");

        if (creatorId.Value != input.RequesterUserId)
            throw new InvalidOperationException("Forbidden."); // map to 403

        var deleted = await _mediaRepo.DeleteAsync(input.MediaId).ConfigureAwait(false);
        if (!deleted)
            throw new KeyNotFoundException("Media not found."); // race: already deleted
    }
}