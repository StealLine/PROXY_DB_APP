using PROXY_TODB.DBmethods;
using PROXY_TODB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEST_PROXY.Fixtures;

namespace TEST_PROXY
{
    [Collection("PostgresCollection")]
    public class RemoveDBactionTests
    {
        private readonly PostgresFixture _fixture;

        public RemoveDBactionTests(PostgresFixture fixture)
        {
            _fixture = fixture;
        }

        private async Task<string> CreateTestDB()
        {
            var (hash, _) = await new CreateDBaction().Create();
            return hash;
        }

        [Fact]
        public async Task Delete_ShouldReturnSuccess_WhenHashExists()
        {
            var hash = await CreateTestDB();

            var result = await new RemoveDBaction().Delete(hash);

            Assert.Equal("Deleted successfully.", result);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenHashDoesNotExist()
        {
            var result = await new RemoveDBaction().Delete("nonexistent_hash_123");

            Assert.Equal("Record not found.", result);
        }

        [Fact]
        public async Task Delete_ShouldRemoveRecordFromManagementDB()
        {
            var hash = await CreateTestDB();
            await new RemoveDBaction().Delete(hash);

            var result = await _fixture.ExecuteScalarAsync(
                FullProgramModel.ManagementConnectionString,
                "SELECT 1 FROM management_items WHERE unique_hash = @hash;",
                new() { { "hash", hash } }
            );

            Assert.Null(result);
        }

        [Fact]
        public async Task Delete_ShouldDropDatabase()
        {
            var hash = await CreateTestDB();
            await new RemoveDBaction().Delete(hash);

            var result = await _fixture.ExecuteScalarAsync(
                FullProgramModel.MasterConnectionString,
                "SELECT 1 FROM pg_database WHERE datname = @dbname;",
                new() { { "dbname", $"preview_{hash}" } }
            );

            Assert.Null(result);
        }

        [Fact]
        public async Task Delete_ShouldDropUser()
        {
            var hash = await CreateTestDB();
            await new RemoveDBaction().Delete(hash);

            var result = await _fixture.ExecuteScalarAsync(
                FullProgramModel.MasterConnectionString,
                "SELECT 1 FROM pg_roles WHERE rolname = @username;",
                new() { { "username", $"user_{hash}" } }
            );

            Assert.Null(result);
        }
    }
}
