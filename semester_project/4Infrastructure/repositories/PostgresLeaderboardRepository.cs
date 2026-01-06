using Npgsql;
using semester_project._2Application.Interfaces;
using semester_project._2Application.UseCases.Leaderboard;
using semester_project._4Infrastructure.data;

namespace semester_project._4Infrastructure.repositories;

public class PostgresLeaderboardRepository(PostgresConnectionFactory factory) : ILeaderboardRepository
{
    public async Task<IReadOnlyList<LeaderboardEntry>> GetLeaderboardAsync()
    {
        // leaderboard sorted by number of ratings per user.
        const string sql = @"
            SELECT
                u.id,
                u.username,
                COUNT(r.id)::int AS total_ratings,
                COALESCE(AVG(r.stars)::float8, 0)::float8 AS avg_stars
            FROM users u
            LEFT JOIN ratings r ON r.user_id = u.id
            GROUP BY u.id, u.username
            ORDER BY total_ratings DESC, avg_stars DESC, u.id DESC;";

        await using var conn = (NpgsqlConnection)factory.Create();
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);

        var items = new List<LeaderboardEntry>();

        await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            var userId = reader.GetInt64(0);
            var username = reader.GetString(1);
            var totalRatings = reader.GetInt32(2);
            var avgStars = reader.GetDouble(3);

            items.Add(new LeaderboardEntry(
                UserId: userId,
                Username: username,
                TotalRatings: totalRatings,
                AverageStars: avgStars
            ));
        }

        return items;
    }
}