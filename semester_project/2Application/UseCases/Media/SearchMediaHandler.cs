using semester_project._2Application.Interfaces;

namespace semester_project._2Application.UseCases.Media;

public class SearchMediaHandler(IMediaRepository mediaRepo)
{
    private static readonly HashSet<string> AllowedSortBy =
        new(StringComparer.OrdinalIgnoreCase) { "title", "year", "score" };

    private static readonly HashSet<string> AllowedTypes =
        new(StringComparer.OrdinalIgnoreCase) { "movie", "series", "game" };

    public Task<IReadOnlyList<MediaSearchItem>> HandleAsync(SearchMediaInput input)
    {
        var sortBy = string.IsNullOrWhiteSpace(input.SortBy) ? "score" : input.SortBy.Trim();

        if (!AllowedSortBy.Contains(sortBy))
            throw new ArgumentException("sortBy must be one of: title | year | score.");

        if (!string.IsNullOrWhiteSpace(input.MediaType) && !AllowedTypes.Contains(input.MediaType.Trim()))
            throw new ArgumentException("mediaType must be one of: movie | series | game.");

        if (input.Rating is not null && (input.Rating.Value < 0 || input.Rating.Value > 5))
            throw new ArgumentException("rating must be between 0 and 5.");

        if (input.AgeRestriction is not null && input.AgeRestriction.Value < 0)
            throw new ArgumentException("ageRestriction must be >= 0.");

        return mediaRepo.SearchAsync(input);
    }
}