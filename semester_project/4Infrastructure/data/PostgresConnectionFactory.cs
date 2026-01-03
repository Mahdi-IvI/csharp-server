using System.Data;
using Npgsql;

namespace semester_project._4Infrastructure.data;

public class PostgresConnectionFactory(string connectionString)
{
    public IDbConnection Create() => new NpgsqlConnection(connectionString);
}