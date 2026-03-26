
namespace PROXY_TODB.DBmethods
{
    using Npgsql;
    using PROXY_TODB.Controllers;
    using PROXY_TODB.DBInterfaces;
    using PROXY_TODB.Models;

    public class ExecScript : IexecScript
    {
        public async Task<(string message, bool success)> ExecuteScriptByHash(string hash, string sqlScript)
        {
            try
            {
                await using var managementDs = NpgsqlDataSource.Create(FullProgramModel.ManagementConnectionString);
                await using var managementConn = await managementDs.OpenConnectionAsync();

                var selectCmd = new NpgsqlCommand(
                    @"
					SELECT related_db_name, related_db_user, related_db_password
					FROM management_items
					WHERE unique_hash = @hash;", managementConn);

                selectCmd.Parameters.AddWithValue("hash", hash);

                await using var reader = await selectCmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return ("Hash not found in management database", false);
                }

                string dbName = reader.GetString(0);
                string username = reader.GetString(1);
                string password = reader.GetString(2);

                await reader.CloseAsync();

                try
                {
                    await using var targetDs = NpgsqlDataSource.Create(
                        $"Host={FullProgramModel.Host};" +
                        $"Port={FullProgramModel.Port};" +
                        $"Username={username};" +
                        $"Password={password};" +
                        $"Database={dbName};");

                    await using var targetConn = await targetDs.OpenConnectionAsync();

                    var scriptCmd = new NpgsqlCommand(sqlScript, targetConn);
                    await scriptCmd.ExecuteNonQueryAsync();

                    return ("Script executed successfully", true);
                }
                catch (NpgsqlException ex) when (ex.SqlState == "28P01")
                {
                    return ("Invalid database credentials", false);
                }
                catch (NpgsqlException ex) when (ex.SqlState == "3D000")
                {
                    return ("Target database does not exist", false);
                }
                catch (NpgsqlException ex) when (ex.SqlState == "42601")
                {
                    return ("SQL syntax error in provided script", false);
                }
                catch (NpgsqlException ex)
                {
                    return ($"PostgreSQL error: {ex.Message}", false);
                }
            }
            catch (NpgsqlException)
            {
                return ("Cannot connect to Management database", false);
            }
            catch (Exception ex)
            {
                return ($"Unexpected error: {ex.Message}", false);
            }
        }
    }
}
