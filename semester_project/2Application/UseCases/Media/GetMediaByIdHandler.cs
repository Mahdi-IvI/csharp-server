using semester_project._1Presentation.Http.Contracts.Media;
using semester_project._2Application.Interfaces;

namespace semester_project._2Application.UseCases.Media;

public sealed class GetMediaByIdHandler(IMediaRepository mediaRepo)
{
    public async Task<MediaDetails> HandleAsync(GetMediaByIdInput input)
    {
        var media = await mediaRepo.GetByIdAsync(input.MediaId).ConfigureAwait(false);
        if (media is null)
            throw new KeyNotFoundException("Media not found.");

        return media;
    }
}