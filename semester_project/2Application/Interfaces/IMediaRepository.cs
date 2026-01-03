using semester_project._1Presentation.Http.Contracts.Media;
using semester_project._2Application.UseCases.Media;
using semester_project._3Domain.Entities;

namespace semester_project._2Application.Interfaces;

public interface IMediaRepository
{
    Task<long> AddAsync(Media media);

    Task<long?> GetCreatorIdAsync(long mediaId);
    Task<bool> DeleteAsync(long mediaId);

    Task<bool> UpdateAsync(Media media);

    Task<long> FavoriteAsync(Favorite favorite);
    Task<bool> UnfavoriteAsync(Favorite favorite);

    Task<long?> GetFavoriteAsync(long mediaId, long userId);

    Task<IReadOnlyList<MediaSearchItem>> SearchAsync(SearchMediaInput input);

    Task<MediaDetails?> GetByIdAsync(long mediaId);
}