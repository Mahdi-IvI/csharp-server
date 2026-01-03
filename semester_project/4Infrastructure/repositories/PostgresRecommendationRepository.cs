using Npgsql;
using semester_project._2Application.Interfaces;
using semester_project._2Application.UseCases.Recommendations;
using semester_project._4Infrastructure.data;

namespace semester_project._4Infrastructure.repositories;

public class PostgresRecommendationRepository(PostgresConnectionFactory factory) : IRecommendationRepository
{
    public async Task<IReadOnlyList<RecommendationItem>> RecommendByGenreAsync(long userId, int limit)
    {
        // 1) Primary: genres from user's highly rated media (>=4)
        var primary = await RecommendByUserHighRatedGenresAsync(userId, limit).ConfigureAwait(false);
        if (primary.Count > 0) return primary;

        // 2) Fallback: user's favorite_genre
        var fav = await RecommendByUserFavoriteGenreAsync(userId, limit).ConfigureAwait(false);
        if (fav.Count > 0) return fav;

        // 3) Fallback: top rated overall
        return await RecommendTopRatedOverallAsync(userId, limit).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<RecommendationItem>> RecommendByContentAsync(long userId, int limit)
    {
        // 1) Primary: similarity based on user's highly rated media (>=4)
        var primary = await RecommendByContentSimilarityAsync(userId, limit).ConfigureAwait(false);
        if (primary.Count > 0) return primary;

        // 2) Fallback: genre-based
        return await RecommendByGenreAsync(userId, limit).ConfigureAwait(false);
    }

    // -----------------------
    // Primary (Genre)
    // -----------------------
    private async Task<List<RecommendationItem>> RecommendByUserHighRatedGenresAsync(long userId, int limit)
    {
        const string sql = @"
WITH user_genres AS (
    SELECT mg.genre, COUNT(*)::int AS cnt
    FROM ratings r
    JOIN media_genres mg ON mg.media_id = r.media_id
    WHERE r.user_id = @user_id AND r.stars >= 4
    GROUP BY mg.genre
),
candidates AS (
    SELECT
        m.id AS media_id,
        m.title,
        m.media_type,
        m.release_year,
        m.age_restriction,
        COALESCE(AVG(r2.stars)::float8, 0)::float8 AS avg_stars,
        ARRAY_REMOVE(ARRAY_AGG(DISTINCT mg2.genre), NULL) AS genres,
        SUM(COALESCE(ug.cnt, 0))::int AS score
    FROM media m
    LEFT JOIN media_genres mg2 ON mg2.media_id = m.id
    LEFT JOIN user_genres ug ON ug.genre = mg2.genre
    LEFT JOIN ratings r2 ON r2.media_id = m.id
    WHERE NOT EXISTS (
        SELECT 1 FROM ratings rx WHERE rx.user_id = @user_id AND rx.media_id = m.id
    )
    GROUP BY m.id, m.title, m.media_type, m.release_year, m.age_restriction
)
SELECT media_id, title, media_type, release_year, age_restriction, genres, avg_stars
FROM candidates
WHERE score > 0
ORDER BY score DESC, avg_stars DESC, media_id DESC
LIMIT @limit;";

        return await QueryRecommendationsAsync(sql, userId, limit).ConfigureAwait(false);
    }

    // -----------------------
    // Primary (Content)
    // -----------------------
    private async Task<List<RecommendationItem>> RecommendByContentSimilarityAsync(long userId, int limit)
    {
        const string sql = @"
WITH liked_media AS (
    SELECT DISTINCT r.media_id
    FROM ratings r
    WHERE r.user_id = @user_id AND r.stars >= 4
),
liked_genres AS (
    SELECT mg.genre, COUNT(*)::int AS cnt
    FROM liked_media lm
    JOIN media_genres mg ON mg.media_id = lm.media_id
    GROUP BY mg.genre
),
liked_types AS (
    SELECT m.media_type, COUNT(*)::int AS cnt
    FROM liked_media lm
    JOIN media m ON m.id = lm.media_id
    GROUP BY m.media_type
),
liked_ages AS (
    SELECT m.age_restriction, COUNT(*)::int AS cnt
    FROM liked_media lm
    JOIN media m ON m.id = lm.media_id
    GROUP BY m.age_restriction
),
candidates AS (
    SELECT
        m.id AS media_id,
        m.title,
        m.media_type,
        m.release_year,
        m.age_restriction,
        COALESCE(AVG(r2.stars)::float8, 0)::float8 AS avg_stars,
        ARRAY_REMOVE(ARRAY_AGG(DISTINCT mg.genre), NULL) AS genres,
        (
            SUM(COALESCE(lg.cnt, 0)) * 2
            + COALESCE(lt.cnt, 0) * 3
            + COALESCE(la.cnt, 0) * 1
        )::int AS score
    FROM media m
    LEFT JOIN media_genres mg ON mg.media_id = m.id
    LEFT JOIN liked_genres lg ON lg.genre = mg.genre
    LEFT JOIN liked_types lt ON lt.media_type = m.media_type
    LEFT JOIN liked_ages la ON la.age_restriction = m.age_restriction
    LEFT JOIN ratings r2 ON r2.media_id = m.id
    WHERE NOT EXISTS (
        SELECT 1 FROM ratings rx WHERE rx.user_id = @user_id AND rx.media_id = m.id
    )
    GROUP BY m.id, m.title, m.media_type, m.release_year, m.age_restriction, lt.cnt, la.cnt
)
SELECT media_id, title, media_type, release_year, age_restriction, genres, avg_stars
FROM candidates
WHERE score > 0
ORDER BY score DESC, avg_stars DESC, media_id DESC
LIMIT @limit;";

        return await QueryRecommendationsAsync(sql, userId, limit).ConfigureAwait(false);
    }

    // -----------------------
    // Fallbacks
    // -----------------------
    private async Task<List<RecommendationItem>> RecommendByUserFavoriteGenreAsync(long userId, int limit)
    {
        const string sql = @"
WITH fav AS (
    SELECT favorite_genre
    FROM users
    WHERE id = @user_id
),
candidates AS (
    SELECT
        m.id AS media_id,
        m.title,
        m.media_type,
        m.release_year,
        m.age_restriction,
        COALESCE(AVG(r2.stars)::float8, 0)::float8 AS avg_stars,
        ARRAY_REMOVE(ARRAY_AGG(DISTINCT mg.genre), NULL) AS genres
    FROM media m
    JOIN media_genres mg ON mg.media_id = m.id
    JOIN fav ON fav.favorite_genre IS NOT NULL AND mg.genre = fav.favorite_genre
    LEFT JOIN ratings r2 ON r2.media_id = m.id
    WHERE NOT EXISTS (
        SELECT 1 FROM ratings rx WHERE rx.user_id = @user_id AND rx.media_id = m.id
    )
    GROUP BY m.id, m.title, m.media_type, m.release_year, m.age_restriction
)
SELECT media_id, title, media_type, release_year, age_restriction, genres, avg_stars
FROM candidates
ORDER BY avg_stars DESC, media_id DESC
LIMIT @limit;";

        return await QueryRecommendationsAsync(sql, userId, limit).ConfigureAwait(false);
    }

    private async Task<List<RecommendationItem>> RecommendTopRatedOverallAsync(long userId, int limit)
    {
        const string sql = @"
WITH candidates AS (
    SELECT
        m.id AS media_id,
        m.title,
        m.media_type,
        m.release_year,
        m.age_restriction,
        COALESCE(AVG(r2.stars)::float8, 0)::float8 AS avg_stars,
        ARRAY_REMOVE(ARRAY_AGG(DISTINCT mg.genre), NULL) AS genres
    FROM media m
    LEFT JOIN media_genres mg ON mg.media_id = m.id
    LEFT JOIN ratings r2 ON r2.media_id = m.id
    WHERE NOT EXISTS (
        SELECT 1 FROM ratings rx WHERE rx.user_id = @user_id AND rx.media_id = m.id
    )
    GROUP BY m.id, m.title, m.media_type, m.release_year, m.age_restriction
)
SELECT media_id, title, media_type, release_year, age_restriction, genres, avg_stars
FROM candidates
ORDER BY avg_stars DESC, media_id DESC
LIMIT @limit;";

        return await QueryRecommendationsAsync(sql, userId, limit).ConfigureAwait(false);
    }

    // -----------------------
    // Shared query runner
    // -----------------------
    private async Task<List<RecommendationItem>> QueryRecommendationsAsync(string sql, long userId, int limit)
    {
        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@user_id", userId);
        cmd.Parameters.AddWithValue("@limit", limit);

        var items = new List<RecommendationItem>();

        await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            var mediaId = reader.GetInt64(0);
            var title = reader.GetString(1);
            var mediaType = reader.GetString(2);
            var releaseYear = reader.IsDBNull(3) ? null : reader.GetString(3);
            var ageRestriction = (short)reader.GetInt16(4);

            List<string> genres;
            if (reader.IsDBNull(5))
            {
                genres = new List<string>();
            }
            else
            {
                // Postgres text[] -> string[]
                var arr = reader.GetFieldValue<string[]>(5);
                genres = arr?.ToList() ?? new List<string>();
            }

            var avgStars = reader.IsDBNull(6) ? 0.0 : reader.GetDouble(6);

            items.Add(new RecommendationItem(
                MediaId: mediaId,
                Title: title,
                MediaType: mediaType,
                ReleaseYear: releaseYear,
                AgeRestriction: ageRestriction,
                Genres: genres,
                AverageStars: avgStars
            ));
        }

        return items;
    }
}