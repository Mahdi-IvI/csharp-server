using semester_project.Application.Ports;
using semester_project.Domain.Entities;
using semester_project.Domain.Enums;

namespace semester_project.Application.UseCases.Media;

public class CreateMediaHandler
{
    private static readonly HashSet<string> AllowedTypes =
        new(StringComparer.OrdinalIgnoreCase) { "movie", "series", "game" };

    private readonly IMediaRepository _mediaRepo;

    public CreateMediaHandler(IMediaRepository mediaRepo)
    {
        _mediaRepo = mediaRepo ?? throw new ArgumentNullException(nameof(mediaRepo));
    }

    public async Task<CreateMediaResult> HandleAsync(CreateMediaInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
        {
            throw new ArgumentException("Title is required.");
        }

        if (!AllowedTypes.Contains(input.MediaType))
            throw new ArgumentException("mediaType must be one of: movie | series | game.");

        var normalizedGenres = (input.Genres ?? Array.Empty<string>())
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Select(g => g.Trim().ToLowerInvariant())
            .Distinct()
            .Select(g => {
                if (Enum.TryParse<Genre>(g.Replace("-", ""), ignoreCase: true, out var result))
                    return result;

                return (Genre?)null;
            })
            .Where(g => g != null)
            .Select(g => g!.Value)
            .ToList();
        
        MediaType? parsedType = null;
        if (!string.IsNullOrWhiteSpace(input.MediaType))
        {
            if (!Enum.TryParse<MediaType>(input.MediaType, ignoreCase: true, out var g))
                throw new ArgumentException("Invalid favoriteGenre value.");
            parsedType = g;
        }

        var media = new Domain.Entities.Media
        (
            0,
            input.Title,
            input.Description ??"",
            parsedType ?? MediaType.movie,
            input.ReleaseYear.ToString(),
            normalizedGenres.ToList(),
            input.AgeRestriction,
            input.CreatedByUserId,
            DateTime.UtcNow
        );

        var newId = await _mediaRepo.AddAsync(media).ConfigureAwait(false);
        return new CreateMediaResult(newId);
    }
}