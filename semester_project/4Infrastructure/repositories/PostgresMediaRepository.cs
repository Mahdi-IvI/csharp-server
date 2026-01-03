using System.Text;
using Npgsql;
using NpgsqlTypes;
using semester_project._1Presentation.Http.Contracts.Media;
using semester_project._2Application.Interfaces;
using semester_project._2Application.UseCases.Media;
using semester_project._3Domain.Entities;
using semester_project._4Infrastructure.data;

namespace semester_project._4Infrastructure.repositories;

public class PostgresMediaRepository(PostgresConnectionFactory factory) : IMediaRepository
{
    public async Task<long> AddAsync(Media media)
    {
        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);
        await using var tx = await conn.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            // Insert media
            const string insertMedia = @"
                    INSERT INTO media (title, description, media_type, release_year, age_restriction, created_by, created_at)
                    VALUES (@title, @description, @media_type, @release_year, @age_restriction, @created_by, NOW())
                    RETURNING id;";

            await using var cmd = new NpgsqlCommand(insertMedia, conn, tx);
            cmd.Parameters.AddWithValue("@title", media.Title);
            cmd.Parameters.AddWithValue("@description", (object?)media.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@media_type", media.Type.ToString());
            cmd.Parameters.AddWithValue("@release_year", (object?)media.ReleaseYear ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@age_restriction", media.AgeRestriction);
            cmd.Parameters.AddWithValue("@created_by", media.CreatedBy);

            var newId = (long)(await cmd.ExecuteScalarAsync().ConfigureAwait(false))!;

            // Insert genres
            const string insertGenre = @"
                    INSERT INTO media_genres (media_id, genre)
                    VALUES (@media_id, @genre)
                    ON CONFLICT DO NOTHING;"; // safe if genre repeated

            foreach (var g in media.Genres)
            {
                await using var gcmd = new NpgsqlCommand(insertGenre, conn, tx);
                gcmd.Parameters.AddWithValue("@media_id", newId);
                gcmd.Parameters.AddWithValue("@genre", g.ToString());
                await gcmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            await tx.CommitAsync().ConfigureAwait(false);
            return newId;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            await tx.RollbackAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task<long?> GetCreatorIdAsync(long mediaId)
    {
        const string sql = "SELECT created_by FROM media WHERE id = @id;";
        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", mediaId);

        var scalar = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        if (scalar is null || scalar is DBNull) return null;
        return (long)scalar;
    }

    public async Task<bool> DeleteAsync(long mediaId)
    {
        const string sql = "DELETE FROM media WHERE id = @id;";
        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", mediaId);

        var rows = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        return rows > 0;
    }

    public async Task<bool> UpdateAsync(Media media)
    {
        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);
        await using var tx = await conn.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            Console.WriteLine(media.Description);
            Console.WriteLine(media.Type);
            Console.WriteLine(media.ReleaseYear);
            Console.WriteLine(media.AgeRestriction);
            Console.WriteLine(media.Id);

            // Insert media
            const string insertMedia = @"
                    UPDATE media 
                    SET title = @title, description = @description, media_type = @media_type, release_year = @release_year, age_restriction = @age_restriction
                    WHERE id = @id
                    RETURNING id;";

            await using var cmd = new NpgsqlCommand(insertMedia, conn, tx);
            cmd.Parameters.AddWithValue("@title", media.Title);
            cmd.Parameters.AddWithValue("@description", (object?)media.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@media_type", media.Type.ToString());
            cmd.Parameters.AddWithValue("@release_year", (object?)media.ReleaseYear ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@age_restriction", media.AgeRestriction);
            cmd.Parameters.AddWithValue("@id", media.Id);

            var id = (long)(await cmd.ExecuteScalarAsync().ConfigureAwait(false))!;


            const string deleteAllGenres = "DELETE FROM media_genres WHERE media_id = @media_id;";
            await using var dgcmd = new NpgsqlCommand(deleteAllGenres, conn, tx);
            dgcmd.Parameters.AddWithValue("@media_id", media.Id);
            await dgcmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            // Insert genres
            const string updateGenre = @"
                    UPDATE media_genres 
                    SET genre = @genre
                    Where media_id = @media_id;"; // safe if genre repeated

            foreach (var g in media.Genres)
            {
                await using var gcmd = new NpgsqlCommand(updateGenre, conn, tx);
                gcmd.Parameters.AddWithValue("@media_id", media.Id);
                gcmd.Parameters.AddWithValue("@genre", g.ToString());
                await gcmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            await tx.CommitAsync().ConfigureAwait(false);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            await tx.RollbackAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task<long> FavoriteAsync(Favorite favorite)
    {
        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);
        await using var tx = await conn.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            // Insert media
            const string insertFavorite = @"
                    INSERT INTO favorites (user_id, media_id)
                    VALUES (@user_id, @media_id)
                    RETURNING user_id, media_id;";

            await using var cmd = new NpgsqlCommand(insertFavorite, conn, tx);
            cmd.Parameters.AddWithValue("@user_id", favorite.UserId);
            cmd.Parameters.AddWithValue("@media_id", favorite.MediaId);


            var newId = (long)(await cmd.ExecuteScalarAsync().ConfigureAwait(false))!;

            await tx.CommitAsync().ConfigureAwait(false);
            return newId;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            await tx.RollbackAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task<bool> UnfavoriteAsync(Favorite favorite)
    {
        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        const string sql = @"
        DELETE FROM favorites
        WHERE media_id = @media_id AND user_id = @user_id;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("user_id", favorite.UserId);
        cmd.Parameters.AddWithValue("media_id", favorite.MediaId);

        var affected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        return affected > 0;
    }

    public async Task<long?> GetFavoriteAsync(long mediaId, long userId)
    {
        const string sql = @"
                SELECT *
                FROM favorites WHERE media_id = @media_id AND  user_id = @user_id;";

        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@media_id", mediaId);
        cmd.Parameters.AddWithValue("@user_id", userId);

        var scalar = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        if (scalar is null || scalar is DBNull) return null;
        return (long)scalar;
    }


    public async Task<IReadOnlyList<MediaSearchItem>> SearchAsync(SearchMediaInput input)
    {
        // DB tables used: media, media_genres, ratings  [oai_citation:2‡schema.sql](file-service://file-UEQM2UTh29im2YidEnqkaV)

        Console.WriteLine(input);

        var sortBy = string.IsNullOrWhiteSpace(input.SortBy) ? "score" : input.SortBy.Trim();

        var orderBy = sortBy.ToLowerInvariant() switch
        {
            "title" => "m.title ASC, m.id DESC",
            "year" => @"(CASE WHEN m.release_year ~ '^\d+$' THEN m.release_year::int END) DESC NULLS LAST,
                     m.title ASC, m.id DESC",
            _ => "score DESC, m.title ASC, m.id DESC"
        };

        var sql = new StringBuilder(@$"
        SELECT
            m.id,
            m.title,
            m.description,
            m.media_type,
            m.release_year,
            m.age_restriction,
            m.score,
            COALESCE(ARRAY_REMOVE(ARRAY_AGG(DISTINCT mg.genre), NULL), '{{}}') AS genres
        FROM media m
        LEFT JOIN ratings r ON r.media_id = m.id
        LEFT JOIN media_genres mg ON mg.media_id = m.id
        WHERE 1=1
          AND (@title IS NULL OR m.title ILIKE '%' || @title || '%')
          AND (@media_type IS NULL OR LOWER(m.media_type) = LOWER(@media_type))
          AND (@release_year IS NULL OR m.release_year = @release_year)
          AND (@age_restriction IS NULL OR m.age_restriction = @age_restriction)
          AND (
               @genre IS NULL OR EXISTS (
                   SELECT 1
                   FROM media_genres mgf
                   WHERE mgf.media_id = m.id
                     AND REPLACE(LOWER(mgf.genre), '-', '') = REPLACE(LOWER(@genre), '-', '')
               )
          )
        GROUP BY m.id
        HAVING (@min_rating IS NULL OR COALESCE(m.score::float8, 0) >= @min_rating)
        ORDER BY {orderBy};
    ");

        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql.ToString(), conn);

        cmd.Parameters.Add("@title", NpgsqlDbType.Text).Value =
            (object?)input.Title?.Trim() ?? DBNull.Value;

        cmd.Parameters.Add("@genre", NpgsqlDbType.Text).Value =
            (object?)input.Genre?.Trim() ?? DBNull.Value;

        cmd.Parameters.Add("@media_type", NpgsqlDbType.Text).Value =
            (object?)input.MediaType?.Trim() ?? DBNull.Value;

        cmd.Parameters.Add("@release_year", NpgsqlDbType.Text).Value =
            input.ReleaseYear is null ? DBNull.Value : input.ReleaseYear.Value.ToString();

        cmd.Parameters.Add("@age_restriction", NpgsqlDbType.Smallint).Value =
            input.AgeRestriction is null ? DBNull.Value : input.AgeRestriction.Value;

        cmd.Parameters.Add("@min_rating", NpgsqlDbType.Double).Value =
            input.Rating is null ? DBNull.Value : input.Rating.Value;

        var items = new List<MediaSearchItem>();

        await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            var id = reader.GetInt64(0);
            var title = reader.GetString(1);
            var description = reader.IsDBNull(2) ? null : reader.GetString(2);
            var mediaType = reader.GetString(3);
            var releaseYear = reader.IsDBNull(4) ? null : reader.GetString(4);
            var ageRestriction = reader.GetInt16(5);
            var score = reader.GetDouble(6);

            // genres is text[]
            var genresArr = reader.IsDBNull(7) ? Array.Empty<string>() : (string[])reader.GetValue(7);
            var genres = genresArr
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .Select(g => g.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            items.Add(new MediaSearchItem(
                Id: id,
                Title: title,
                Description: description,
                MediaType: mediaType,
                ReleaseYear: releaseYear,
                AgeRestriction: ageRestriction,
                Genres: genres,
                Score: score
            ));
        }

        return items;
    }
    
    public async Task<MediaDetails?> GetByIdAsync(long mediaId)
    {
        const string sql = @"
            SELECT
                m.id,
                m.title,
                m.description,
                m.media_type,
                m.release_year,
                m.age_restriction,
                m.created_by,
                m.created_at,
                m.score,
                COALESCE(
                    ARRAY_AGG(DISTINCT mg.genre) FILTER (WHERE mg.genre IS NOT NULL),
                    ARRAY[]::text[]
                ) AS genres,
                COUNT(r.id)::int AS rating_count
            FROM media m
            LEFT JOIN media_genres mg ON mg.media_id = m.id
            LEFT JOIN ratings r ON r.media_id = m.id AND r.is_comment_confirmed = TRUE
            WHERE m.id = @id
            GROUP BY m.id;";

        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", mediaId);

        await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
        if (!await reader.ReadAsync().ConfigureAwait(false))
            return null;

        var genres = reader.GetFieldValue<string[]>(reader.GetOrdinal("genres"));

        double? avg = null;
        var avgOrdinal = reader.GetOrdinal("score");
        if (!reader.IsDBNull(avgOrdinal))
            avg = reader.GetDouble(avgOrdinal);

        var ratingCount = reader.GetInt32(reader.GetOrdinal("rating_count"));

        return new MediaDetails(
            Id: reader.GetInt64(reader.GetOrdinal("id")),
            Title: reader.GetString(reader.GetOrdinal("title")),
            Description: reader.IsDBNull(reader.GetOrdinal("description"))
                ? null
                : reader.GetString(reader.GetOrdinal("description")),
            MediaType: reader.GetString(reader.GetOrdinal("media_type")),
            ReleaseYear: reader.IsDBNull(reader.GetOrdinal("release_year"))
                ? null
                : reader.GetString(reader.GetOrdinal("release_year")),
            AgeRestriction: reader.GetInt16(reader.GetOrdinal("age_restriction")),
            CreatedBy: reader.GetInt64(reader.GetOrdinal("created_by")),
            CreatedAt: reader.GetDateTime(reader.GetOrdinal("created_at")),
            Genres: genres,
            AverageScore: avg,
            RatingCount: ratingCount
        );
    }
}