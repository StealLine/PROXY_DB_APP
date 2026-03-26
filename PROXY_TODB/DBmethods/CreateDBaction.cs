namespace PROXY_TODB.DBmethods
{
	using System.Security.Cryptography;
	using System.Text;
	using Npgsql;
	using PROXY_TODB.DBInterfaces;
	using PROXY_TODB.Models;

	public class CreateDBaction : IcreateDB
	{
		public async Task<(string, string)> Create()
		{
			string hash = this.GenerateHash(12);

			string dbName = $"preview_{hash}";
			string username = $"user_{hash}";
			string password = this.GenerateSecurePassword(20);

			await using var masterDataSource = NpgsqlDataSource.Create(FullProgramModel.MasterConnectionString);
			await using var masterConn = await masterDataSource.OpenConnectionAsync();

			if (await this.DatabaseExists(masterConn, dbName))
			{
				return (string.Empty, "Database already exists. Probably hash collision. Try again");
			}

			if (await this.UserExists(masterConn, username))
			{
				return (string.Empty, "User already exists. Probably hash collision. Try again");
			}


			var createUserSqlCmd = new NpgsqlCommand(
				"SELECT format('CREATE USER %I WITH PASSWORD %L', @username, @password)",
				masterConn);
			createUserSqlCmd.Parameters.AddWithValue("username", username);
			createUserSqlCmd.Parameters.AddWithValue("password", password);

			var createUserSql = (string)(await createUserSqlCmd.ExecuteScalarAsync())!;
			await new NpgsqlCommand(createUserSql, masterConn).ExecuteNonQueryAsync();


			var createDbSqlCmd = new NpgsqlCommand(
				"SELECT format('CREATE DATABASE %I OWNER %I', @dbname, @username)",
				masterConn);
			createDbSqlCmd.Parameters.AddWithValue("dbname", dbName);
			createDbSqlCmd.Parameters.AddWithValue("username", username);

			var createDbSql = (string)(await createDbSqlCmd.ExecuteScalarAsync())!;
			await new NpgsqlCommand(createDbSql, masterConn).ExecuteNonQueryAsync();

			await using var managementDataSource = NpgsqlDataSource.Create(FullProgramModel.ManagementConnectionString);
			await using var managementConn = await managementDataSource.OpenConnectionAsync();

			var insertCmd = new NpgsqlCommand(
				@"
                INSERT INTO management_items 
                    (unique_hash, related_db_name, related_db_user, related_db_password)
                VALUES 
                    (@hash, @dbname, @user, @password);",
				managementConn);

			insertCmd.Parameters.AddWithValue("hash", hash);
			insertCmd.Parameters.AddWithValue("dbname", dbName);
			insertCmd.Parameters.AddWithValue("user", username);
			insertCmd.Parameters.AddWithValue("password", password);

			await insertCmd.ExecuteNonQueryAsync();

			return (hash, "Ok");
		}

		private async Task<bool> DatabaseExists(NpgsqlConnection conn, string dbName)
		{
			var cmd = new NpgsqlCommand(
				"SELECT 1 FROM pg_database WHERE datname = @dbname",
				conn);

			cmd.Parameters.AddWithValue("dbname", dbName);

			return await cmd.ExecuteScalarAsync() != null;
		}

		private async Task<bool> UserExists(NpgsqlConnection conn, string username)
		{
			var cmd = new NpgsqlCommand(
				"SELECT 1 FROM pg_roles WHERE rolname = @username",
				conn);

			cmd.Parameters.AddWithValue("username", username);

			return await cmd.ExecuteScalarAsync() != null;
		}

		private string GenerateHash(int length)
		{
			const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
			var data = new byte[length];

			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(data);

			var result = new StringBuilder(length);
			foreach (var b in data)
			{
				result.Append(chars[b % chars.Length]);
			}

			return result.ToString();
		}

		private string GenerateSecurePassword(int length)
		{
			const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
			var data = new byte[length];

			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(data);

			var result = new StringBuilder(length);
			foreach (var b in data)
			{
				result.Append(chars[b % chars.Length]);
			}

			return result.ToString();
		}
	}
}