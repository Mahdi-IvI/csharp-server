using System.Text;
using Npgsql;
using semester_project._2Application.Interfaces;
using semester_project._3Domain.Entities;
using semester_project._3Domain.Enums;
using semester_project._4Infrastructure.data;

namespace semester_project._4Infrastructure.repositories;

public sealed class PostgresUserRepository(PostgresConnectionFactory factory) : IUserRepository
{
    public async Task<User?> GetByIdAsync(long id)
    {
        const string sql = @"
                SELECT *
                FROM users WHERE id = @id;";

        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
        return await reader.ReadAsync().ConfigureAwait(false) ? Map(reader) : null;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        const string sql = @"
                SELECT * FROM users WHERE username = @username;";

        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@username", username);

        await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
        return await reader.ReadAsync().ConfigureAwait(false) ? Map(reader) : null;
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        const string sql = "SELECT 1 FROM users WHERE username = @username;";
        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@username", username);

        var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        return result is int or long or decimal or double;
    }

    public async Task<long> AddAsync(User user)
    {
        const string sql = @"
                INSERT INTO users (username, password, first_name, last_name)
                VALUES (@username, @password, @first_name, @last_name)
                RETURNING id;";

        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@password", user.Password);
        cmd.Parameters.AddWithValue("@first_name", (object?)user.FirstName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@last_name", (object?)user.LastName ?? DBNull.Value);

        var newId = (long)(await cmd.ExecuteScalarAsync().ConfigureAwait(false))!;
        return newId;
    }

    private static User Map(NpgsqlDataReader reader)
    {
        Console.WriteLine("try to map user");
        int ixId = reader.GetOrdinal("id");
        int ixUname = reader.GetOrdinal("username");
        int ixPassword = reader.GetOrdinal("password");
        int ixFname = reader.GetOrdinal("first_name");
        int ixLname = reader.GetOrdinal("last_name");
        int ixCAt = reader.GetOrdinal("created_at");
        int ixFav = reader.GetOrdinal("favorite_genre");
        string? fav = reader.IsDBNull(ixFav) ? null : reader.GetString(ixFav);

        Genre? favEnum = null;
        if (!string.IsNullOrWhiteSpace(fav) &&
            Enum.TryParse<Genre>(fav, true, out var parsed))
            favEnum = parsed;

        return new User
        (
            reader.GetInt64(ixId),
            reader.GetString(ixUname),
            reader.GetString(ixPassword),
            reader.IsDBNull(ixFname) ? null : reader.GetString(ixFname),
            reader.IsDBNull(ixLname) ? null : reader.GetString(ixLname),
            favEnum,
            reader.GetFieldValue<DateTime>(ixCAt)
        );
    }

    public async Task<bool> UpdateProfileAsync(long id, string? email, string? firstName, string? lastName,
        Genre? genre)
    {
        // Build SET clause only for provided fields
        var set = new StringBuilder();
        using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand();
        cmd.Connection = conn;

        if (!string.IsNullOrWhiteSpace(email))
        {
            set.Append(set.Length == 0 ? " email = @email" : ", email = @email");
            cmd.Parameters.AddWithValue("@email", email);
        }

        if (!string.IsNullOrWhiteSpace(firstName))
        {
            set.Append(set.Length == 0 ? " first_name = @first_name" : ", first_name = @first_name");
            cmd.Parameters.AddWithValue("@first_name", firstName);
        }

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            set.Append(set.Length == 0 ? " last_name = @last_name" : ", last_name = @last_name");
            cmd.Parameters.AddWithValue("@last_name", lastName);
        }

        if (genre.HasValue)
        {
            set.Append(set.Length == 0 ? " genre = @favorite_genre" : ", favorite_genre = @favorite_genre");
            cmd.Parameters.AddWithValue("@favorite_genre", genre.Value.ToString());
        }

        // If nothing to update (should be caught earlier), just return false
        if (set.Length == 0) return false;

        var sql = $@"
                UPDATE users
                   SET{set}
                 WHERE id = @id;";
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@id", id);

        var rows = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        return rows > 0;
    }
}