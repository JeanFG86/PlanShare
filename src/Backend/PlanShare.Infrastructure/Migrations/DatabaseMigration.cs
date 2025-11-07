using Dapper;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace PlanShare.Infrastructure.Migrations;

public static class DatabaseMigration
{
    public static void Migrate(string connectionString, IServiceProvider serviceProvider)
    {
        EnsureDatabaseCreatedForPostgreSql(connectionString);
        MigrateDatabase(serviceProvider);
    }
    
    private static void MigrateDatabase(IServiceProvider serviceProvider)
    {
        var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
        runner.ListMigrations();
        runner.MigrateUp();
    }

    private static void EnsureDatabaseCreatedForPostgreSql(string connectionString)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = connectionStringBuilder.Database;

        connectionStringBuilder.Database = "postgres";

        using var dbConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
        dbConnection.Open();

        var exists = dbConnection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM pg_database WHERE datname = @dbName;",
            new { dbName = databaseName });

        if (exists == 0)
        {
            dbConnection.Execute($"CREATE DATABASE \"{databaseName}\";");
        }
    }
}
