using semester_project._2Application.Interfaces;
using semester_project._3Domain.Enums;

namespace semester_project._2Application.UseCases.Media;

public class UpdateMediaHandler(IMediaRepository mediaRepo)
{
    private static readonly HashSet<string> AllowedTypes =
        new(StringComparer.OrdinalIgnoreCase) { "movie", "series", "game" };


    public async Task HandleAsync(UpdateMediaInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
        {
            throw new ArgumentException("Title is required.");
        }

        if (!AllowedTypes.Contains(input.MediaType))
            throw new ArgumentException("mediaType must be one of: movie | series | game.");

        var normalizedGenres = input.Genres
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Select(g => g.Trim().ToLowerInvariant())
            .Distinct()
            .Select(g =>
            {
                if (Enum.TryParse<Genre>(g.Replace("-", ""), ignoreCase: true, out var result))
                    return result;

                return (Genre?)null;
            })
            .Where(g => g != null)
            .Select(g => g!.Value)
            .ToList();

        var creatorId = await mediaRepo.GetCreatorIdAsync(input.MediaId).ConfigureAwait(false);

        if (creatorId is null)
            throw new KeyNotFoundException("Media not found.");

        if (creatorId.Value != input.CreatedByUserId)
            throw new InvalidOperationException("Forbidden."); // map to 403


        MediaType? parsedType = null;
        if (!string.IsNullOrWhiteSpace(input.MediaType))
        {
            if (!Enum.TryParse<MediaType>(input.MediaType, ignoreCase: true, out var g))
                throw new ArgumentException("Invalid favoriteGenre value.");
            parsedType = g;
        }

        var media = new _3Domain.Entities.Media
        (
            input.MediaId,
            input.Title,
            input.Description ?? "",
            parsedType ?? MediaType.movie,
            (input.ReleaseYear ?? 2025).ToString(),
            normalizedGenres.ToList(),
            input.AgeRestriction,
            input.CreatedByUserId,
            DateTime.Now
        );

        await mediaRepo.UpdateAsync(media).ConfigureAwait(false);
    }
}