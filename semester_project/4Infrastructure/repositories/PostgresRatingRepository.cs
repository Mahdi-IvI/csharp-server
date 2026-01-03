using semester_project._2Application.Interfaces;
using semester_project._3Domain.Entities;
using semester_project._4Infrastructure.data;
using Npgsql;
using semester_project._2Application.UseCases.Ratings;

namespace semester_project._4Infrastructure.repositories;

public class PostgresRatingRepository(PostgresConnectionFactory factory) : IRatingRepository
{
    public async Task<long> AddAsync(Rate rate)
    {
        const string sql = @"
            INSERT INTO ratings (media_id, user_id, stars, comment, is_comment_confirmed)
            VALUES (@media_id, @user_id, @stars, @comment, FALSE)
            RETURNING id;";

        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@media_id", rate.MediaId);
        cmd.Parameters.AddWithValue("@user_id", rate.UserId);
        cmd.Parameters.AddWithValue("@stars", rate.Stars);
        cmd.Parameters.AddWithValue("@comment", (object?)rate.Comment ?? DBNull.Value);

        return (long)(await cmd.ExecuteScalarAsync().ConfigureAwait(false))!;
    }

    public async Task<(long userId, long mediaId)?> GetOwnerAndMediaAsync(long ratingId)
    {
        const string sql = "SELECT user_id, media_id FROM ratings WHERE id=@id;";
        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", ratingId);

        await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
        if (!await reader.ReadAsync().ConfigureAwait(false)) return null;

        return (reader.GetInt64(0), reader.GetInt64(1));
    }

    public async Task<bool> UpdateAsync(long ratingId, short stars, string? comment)
    {
        const string sql = @"
            UPDATE ratings
               SET stars = @stars,
                   comment = @comment,
                   is_comment_confirmed = false
             WHERE id = @id;";

        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", ratingId);
        cmd.Parameters.AddWithValue("@stars", stars);
        cmd.Parameters.AddWithValue("@comment", (object?)comment ?? DBNull.Value);

        return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false) > 0;
    }

    public async Task<bool> DeleteAsync(long ratingId)
    {
        const string sql = "DELETE FROM ratings WHERE id=@id;";
        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", ratingId);

        return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false) > 0;
    }

    public async Task<bool> SetCommentConfirmedAsync(long ratingId)
    {
        const string sql = "UPDATE ratings SET is_comment_confirmed=@c WHERE id=@id;";
        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", ratingId);
        cmd.Parameters.AddWithValue("@c", true);

        return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false) > 0;
    }

    public async Task<long> LikeRateAsync(Like like)
    {
        const string sql = @"
            INSERT INTO rating_likes (rating_id, user_id)
            VALUES (@rating_id, @user_id)
            ON CONFLICT (rating_id, user_id) DO NOTHING
            RETURNING rating_id, user_id;";

        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@rating_id", like.RatingId);
        cmd.Parameters.AddWithValue("@user_id", like.UserId);

        return (long)(await cmd.ExecuteScalarAsync().ConfigureAwait(false))!;
    }

    public async Task<long?> GetRateByIdAsync(long rateId)
    {
        const string sql = @"
                SELECT *
                FROM ratings WHERE id = @id;";

        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", rateId);

        var scalar = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        if (scalar is null || scalar is DBNull) return null;
        return (long)scalar;
    }

    public async Task<long?> GetLikeAsync(long rateId, long userId)
    {
        const string sql = @"
                SELECT *
                FROM rating_likes WHERE rating_id = @rating_id AND  user_id = @user_id;";

        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@rating_id", rateId);
        cmd.Parameters.AddWithValue("@user_id", userId);

        var scalar = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        if (scalar is null || scalar is DBNull) return null;
        return (long)scalar;
    }

    public async Task<IReadOnlyList<GetUserRatingHistoryItem>> ListByUserAsync(long userId)
    {
        const string sql = @"
            SELECT 
                r.id,
                r.media_id,
                m.title,
                r.stars,
                r.comment,
                r.is_comment_confirmed,
                r.created_at
            FROM ratings r
            JOIN media m ON m.id = r.media_id
            WHERE r.user_id = @user_id
            ORDER BY r.created_at DESC, r.id DESC;";

        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@user_id", userId);

        var items = new List<GetUserRatingHistoryItem>();

        await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            var ratingId = reader.GetInt64(0);
            var mediaId = reader.GetInt64(1);
            var mediaTitle = reader.GetString(2);
            var stars = reader.GetInt16(3); // smallint -> Int16
            var comment = reader.IsDBNull(4) ? null : reader.GetString(4);
            var confirmed = reader.GetBoolean(5);
            var createdAt = reader.GetDateTime(6);

            items.Add(new GetUserRatingHistoryItem(
                RatingId: ratingId,
                MediaId: mediaId,
                MediaTitle: mediaTitle,
                Stars: stars,
                Comment: comment,
                IsCommentConfirmed: confirmed,
                CreatedAt: createdAt
            ));
        }

        return items;
    }
}