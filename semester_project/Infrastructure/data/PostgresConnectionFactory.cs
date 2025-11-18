using System.Data;
using Npgsql;

namespace semester_project.Infrastructure.data;

public class PostgresConnectionFactory
{
    private readonly string _connectionString;
    public PostgresConnectionFactory(string connectionString)
        => _connectionString = connectionString;

    public IDbConnection Create() => new NpgsqlConnection(_connectionString);
}