using Npgsql;
using semester_project._2Application.Interfaces;
using semester_project._2Application.UseCases.Favorites;
using semester_project._4Infrastructure.data;

namespace semester_project._4Infrastructure.repositories;

public class PostgresFavoriteRepository(PostgresConnectionFactory factory) : IFavoriteRepository
{
    public async Task<IReadOnlyList<GetUserFavoriteItem>> ListByUserAsync(long userId)
    {
        const string sql = @"
            SELECT
                f.media_id,
                m.title,
                m.media_type,
                m.release_year,
                m.age_restriction,
                f.created_at
            FROM favorites f
            JOIN media m ON m.id = f.media_id
            WHERE f.user_id = @user_id
            ORDER BY f.created_at DESC, f.media_id DESC;";

        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@user_id", userId);

        var items = new List<GetUserFavoriteItem>();

        await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            var mediaId = reader.GetInt64(0);
            var title = reader.GetString(1);
            var mediaType = reader.GetString(2);
            var releaseYear = reader.IsDBNull(3) ? null : reader.GetString(3);
            var ageRestriction = reader.GetInt16(4); // smallint -> Int16
            var favoritedAt = reader.GetDateTime(5);

            items.Add(new GetUserFavoriteItem(
                MediaId: mediaId,
                Title: title,
                MediaType: mediaType,
                ReleaseYear: releaseYear,
                AgeRestriction: ageRestriction,
                FavoritedAt: favoritedAt
            ));
        }

        return items;
    }
}