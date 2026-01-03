using semester_project._2Application.UseCases.Ratings;
using semester_project._3Domain.Entities;

namespace semester_project._2Application.Interfaces;

public interface IRatingRepository
{
    Task<long> AddAsync(Rate rata);

    Task<(long userId, long mediaId)?> GetOwnerAndMediaAsync(long ratingId);

    Task<bool> UpdateAsync(long ratingId, short stars, string? comment);

    Task<bool> DeleteAsync(long ratingId);

    Task<bool> SetCommentConfirmedAsync(long ratingId);

    Task<long> LikeRateAsync(Like like);

    Task<long?> GetRateByIdAsync(long rateId);

    Task<long?> GetLikeAsync(long rateId, long userId);

    Task<IReadOnlyList<GetUserRatingHistoryItem>> ListByUserAsync(long userId);
}