
namespace PROXY_TODB.InitService
{
	using System.Text.RegularExpressions;
	using Microsoft.Extensions.Hosting;
	using Npgsql;
	using PROXY_TODB.Models;

	public class InitManagamentDB : IHostedService
	{
		private static readonly Regex SafeIdentifierRegex = new(@"^[a-zA-Z_][a-zA-Z0-9_]{0,62}$", RegexOptions.Compiled);

		private static string ValidateIdentifier(string identifier, string paramName)
		{
			if (string.IsNullOrWhiteSpace(identifier))
				throw new ArgumentException($"{paramName} cant be empty", paramName);

			if (!SafeIdentifierRegex.IsMatch(identifier))
				throw new ArgumentException(
					$"{paramName} have characters that forbidden '{identifier}'. " +
					"only letters numbers and _", paramName);

			return identifier;
		}

		private NpgsqlDataSource CreateDataSource(string connectionString)
			=> NpgsqlDataSource.Create(connectionString);

		public async Task StartAsync(CancellationToken cancellationToken)
		{

			var safeDbName = ValidateIdentifier(FullProgramModel.ManagementDBName, nameof(FullProgramModel.ManagementDBName));


			await using var dataSource = this.CreateDataSource(FullProgramModel.MasterConnectionString);
			await using var conn = await dataSource.OpenConnectionAsync(cancellationToken);


			await using var checkCmd = new NpgsqlCommand(
				"SELECT 1 FROM pg_database WHERE datname = @dbname", conn);
			checkCmd.Parameters.AddWithValue("dbname", safeDbName);

			var exists = await checkCmd.ExecuteScalarAsync(cancellationToken);
			if (exists == null)
			{

				await using var createDbCmd = new NpgsqlCommand(
					$"CREATE DATABASE \"{safeDbName}\"", conn);
				await createDbCmd.ExecuteNonQueryAsync(cancellationToken);
			}

			FullProgramModel.ManagementConnectionString =
				$"Host={FullProgramModel.Host};" +
				$"Port={FullProgramModel.Port};" +
				$"Username=postgres;" +
				$"Password=postgres;" +
				$"Database={safeDbName}";

			await using var dataSource2 = this.CreateDataSource(FullProgramModel.ManagementConnectionString);
			await using var conn2 = await dataSource2.OpenConnectionAsync(cancellationToken);

			await using var createTableCmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS management_items (
                    id                  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    unique_hash         TEXT UNIQUE,
                    related_db_name     TEXT UNIQUE,
                    related_db_user     TEXT UNIQUE,
                    related_db_password TEXT UNIQUE
                );", conn2);
			await createTableCmd.ExecuteNonQueryAsync(cancellationToken);
		}

		public Task StopAsync(CancellationToken cancellationToken)
			=> Task.CompletedTask;
	}
}