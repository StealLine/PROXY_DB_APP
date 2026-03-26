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
    public class CreateDBactionTests : IAsyncDisposable
    {
        private readonly PostgresFixture _fixture;
        private readonly List<string> _createdHashes = new(); 

        public CreateDBactionTests(PostgresFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Create_ShouldReturnHashAndOk()
        {
            var action = new CreateDBaction();

            var (hash, message) = await action.Create();
            _createdHashes.Add(hash);

            Assert.False(string.IsNullOrEmpty(hash));
            Assert.Equal("Ok", message);
        }

        [Fact]
        public async Task Create_ShouldInsertRecordIntoManagementDB()
        {
            var action = new CreateDBaction();
            var (hash, _) = await action.Create();
            _createdHashes.Add(hash);

            var result = await _fixture.ExecuteScalarAsync(
                FullProgramModel.ManagementConnectionString,
                "SELECT 1 FROM management_items WHERE unique_hash = @hash;",
                new() { { "hash", hash } }
            );

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Create_ShouldCreateActualDatabase()
        {
            var action = new CreateDBaction();
            var (hash, _) = await action.Create();
            _createdHashes.Add(hash);

            var result = await _fixture.ExecuteScalarAsync(
                FullProgramModel.MasterConnectionString,
                "SELECT 1 FROM pg_database WHERE datname = @dbname;",
                new() { { "dbname", $"preview_{hash}" } }
            );

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Create_ShouldCreateActualUser()
        {
            var action = new CreateDBaction();
            var (hash, _) = await action.Create();
            _createdHashes.Add(hash);

            var result = await _fixture.ExecuteScalarAsync(
                FullProgramModel.MasterConnectionString,
                "SELECT 1 FROM pg_roles WHERE rolname = @username;",
                new() { { "username", $"user_{hash}" } }
            );

            Assert.NotNull(result);
        }


        public async ValueTask DisposeAsync()
        {
            var remove = new RemoveDBaction();
            foreach (var hash in _createdHashes)
            {
                try { await remove.Delete(hash); }
                catch {}
            }
        }
    }
}
