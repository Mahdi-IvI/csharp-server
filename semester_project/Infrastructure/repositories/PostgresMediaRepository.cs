using Npgsql;
using semester_project.Application.Ports;
using semester_project.Domain.Entities;
using semester_project.Infrastructure.data;

namespace semester_project.Infrastructure.repositories;

public class PostgresMediaRepository : IMediaRepository
{
    private readonly PostgresConnectionFactory _factory;

    public PostgresMediaRepository(PostgresConnectionFactory factory)
        => _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    public async Task<long> AddAsync(Media media)
    {
        await using var conn = (NpgsqlConnection)_factory.Create();
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
            cmd.Parameters.AddWithValue("@release_year", (object?)media.ReleaseYear.ToString() ?? DBNull.Value);
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
        await using var conn = (NpgsqlConnection)_factory.Create();
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
        await using var conn = (NpgsqlConnection)_factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", mediaId);

        var rows = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        return rows > 0;
    }
}