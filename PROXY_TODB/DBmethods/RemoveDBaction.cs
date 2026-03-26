namespace PROXY_TODB.DBmethods
{
    using Microsoft.Extensions.Configuration;
    using Npgsql;
    using PROXY_TODB.Controllers;
    using PROXY_TODB.DBInterfaces;
    using PROXY_TODB.Models;

    public class RemoveDBaction : IremoveDB
    {
        public async Task<string> Delete(string hash)
        {

            await using var managementDs = NpgsqlDataSource.Create(FullProgramModel.ManagementConnectionString);
            await using var managementConn = await managementDs.OpenConnectionAsync();

            var selectCmd = new NpgsqlCommand(
                @"
                SELECT related_db_name, related_db_user
                FROM management_items
                WHERE unique_hash = @hash;", managementConn);
            selectCmd.Parameters.AddWithValue("hash", hash);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return "Record not found.";
            }

            string dbName = reader.GetString(0);
            string username = reader.GetString(1);
            await reader.CloseAsync();


            await using var masterDs = NpgsqlDataSource.Create(FullProgramModel.MasterConnectionString);
            await using var masterConn = await masterDs.OpenConnectionAsync();

			if (await this.DatabaseExists(masterConn, dbName))
			{
				var terminateCmd = new NpgsqlCommand(
					@"SELECT pg_terminate_backend(pid)
                      FROM pg_stat_activity
                      WHERE datname = @dbname;", masterConn);
				terminateCmd.Parameters.AddWithValue("dbname", dbName);
				await terminateCmd.ExecuteNonQueryAsync();

				var dropDbSqlCmd = new NpgsqlCommand(
					"SELECT format('DROP DATABASE %I', @dbname)", masterConn);
				dropDbSqlCmd.Parameters.AddWithValue("dbname", dbName);
				var dropDbSql = (string)(await dropDbSqlCmd.ExecuteScalarAsync())!;
				await new NpgsqlCommand(dropDbSql, masterConn).ExecuteNonQueryAsync();
			}  


			if (await this.UserExists(masterConn, username))
			{
				var dropUserSqlCmd = new NpgsqlCommand(
					"SELECT format('DROP USER %I', @username)", masterConn);
				dropUserSqlCmd.Parameters.AddWithValue("username", username);
				var dropUserSql = (string)(await dropUserSqlCmd.ExecuteScalarAsync())!;

				await new NpgsqlCommand(dropUserSql, masterConn).ExecuteNonQueryAsync();
			}


			var deleteRecordCmd = new NpgsqlCommand(
                @"
                DELETE FROM management_items
                WHERE unique_hash = @hash;", managementConn);
            deleteRecordCmd.Parameters.AddWithValue("hash", hash);
            await deleteRecordCmd.ExecuteNonQueryAsync();

            return "Deleted successfully.";
        }

        private async Task<bool> DatabaseExists(NpgsqlConnection conn, string dbName)
        {
            var cmd = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @dbname;", conn);
            cmd.Parameters.AddWithValue("dbname", dbName);
            return await cmd.ExecuteScalarAsync() != null;
        }

        private async Task<bool> UserExists(NpgsqlConnection conn, string username)
        {
            var cmd = new NpgsqlCommand("SELECT 1 FROM pg_roles WHERE rolname = @username;", conn);
            cmd.Parameters.AddWithValue("username", username);
            return await cmd.ExecuteScalarAsync() != null;
        }
    }
}