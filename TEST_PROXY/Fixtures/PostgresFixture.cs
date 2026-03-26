using Npgsql;
using PROXY_TODB.InitService;
using PROXY_TODB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEST_PROXY.Fixtures
{
	public class PostgresFixture : IAsyncLifetime
	{
		private string Host => Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
		private string Port => Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
		private string DbName => Environment.GetEnvironmentVariable("DBNAME") ?? "ManagementDB";

		public async Task InitializeAsync()
		{
			FullProgramModel.Host = Host;
			FullProgramModel.Port = Port;
			FullProgramModel.ManagementDBName = DbName;
			FullProgramModel.MasterConnectionString =
				$"Host={Host};Port={Port};Username=postgres;Password=postgres;Database=postgres";

			for (int i = 0; i < 30; i++)
			{
				try
				{
					await using var ds = NpgsqlDataSource.Create(FullProgramModel.MasterConnectionString);
					await using var conn = await ds.OpenConnectionAsync();
					break;
				}
				catch
				{
					if (i == 29) throw new Exception($"Postgres havent responded {Host}:{Port}");
					await Task.Delay(1000);
				}
			}

			var init = new InitManagamentDB();
			await init.StartAsync(CancellationToken.None);
		}

		public Task DisposeAsync() => Task.CompletedTask;

		public async Task<object?> ExecuteScalarAsync(string connectionString, string sql, Dictionary<string, object>? parameters = null)
		{
			await using var ds = NpgsqlDataSource.Create(connectionString);
			await using var conn = await ds.OpenConnectionAsync();
			var cmd = new NpgsqlCommand(sql, conn);
			if (parameters != null)
				foreach (var p in parameters)
					cmd.Parameters.AddWithValue(p.Key, p.Value);
			return await cmd.ExecuteScalarAsync();
		}
	}
}
