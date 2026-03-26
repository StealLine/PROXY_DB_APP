using PROXY_TODB.DBmethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEST_PROXY.Fixtures;

namespace TEST_PROXY
{
    [Collection("PostgresCollection")]
    public class ExecScriptTests : IAsyncDisposable
    {
        private readonly PostgresFixture _fixture;
        private string? _testHash;

        public ExecScriptTests(PostgresFixture fixture)
        {
            _fixture = fixture;
        }

        private async Task<string> GetOrCreateTestHash()
        {
            if (_testHash != null) return _testHash;
            var (hash, _) = await new CreateDBaction().Create();
            _testHash = hash;
            return hash;
        }

        [Fact]
        public async Task ExecuteScript_ShouldReturnSuccess_ForValidSQL()
        {
            var hash = await GetOrCreateTestHash();
            var exec = new ExecScript();

            var (message, success) = await exec.ExecuteScriptByHash(hash,
                "CREATE TABLE IF NOT EXISTS test_table (id SERIAL PRIMARY KEY, name TEXT);");

            Assert.True(success);
            Assert.Equal("Script executed successfully", message);
        }

        [Fact]
        public async Task ExecuteScript_ShouldReturnError_ForInvalidSQL()
        {
            var hash = await GetOrCreateTestHash();
            var exec = new ExecScript();

            var (message, success) = await exec.ExecuteScriptByHash(hash,
                "THIS IS NOT VALID SQL !!!;");

            Assert.False(success);
            Assert.Equal("SQL syntax error in provided script", message);
        }

        [Fact]
        public async Task ExecuteScript_ShouldReturnHashNotFound_ForUnknownHash()
        {
            var exec = new ExecScript();

            var (message, success) = await exec.ExecuteScriptByHash("fake_hash_000",
                "SELECT 1;");

            Assert.False(success);
            Assert.Equal("Hash not found in management database", message);
        }

        public async ValueTask DisposeAsync()
        {
            if (_testHash != null)
            {
                try { await new RemoveDBaction().Delete(_testHash); }
                catch { }
            }
        }
    }
}
