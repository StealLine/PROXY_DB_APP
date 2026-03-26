using PROXY_TODB.InitService;
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
    public class InitManagamentDBTests
    {
        private readonly PostgresFixture _fixture;

        public InitManagamentDBTests(PostgresFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task StartAsync_ShouldCreateManagementDatabase()
        {
            var result = await _fixture.ExecuteScalarAsync(
                FullProgramModel.MasterConnectionString,
                "SELECT 1 FROM pg_database WHERE datname = @dbname;",
                new() { { "dbname", FullProgramModel.ManagementDBName } }
            );

            Assert.NotNull(result);
        }

        [Fact]
        public async Task StartAsync_ShouldCreateManagementItemsTable()
        {
            var result = await _fixture.ExecuteScalarAsync(
                FullProgramModel.ManagementConnectionString,
                "SELECT 1 FROM information_schema.tables WHERE table_name = 'management_items';"
            );

            Assert.NotNull(result);
        }

        [Fact]
        public async Task StartAsync_ShouldBeIdempotent_WhenCalledTwice()
        {
            var init = new InitManagamentDB();


            var ex = await Record.ExceptionAsync(() =>
                init.StartAsync(CancellationToken.None));

            Assert.Null(ex);
        }

        [Fact]
        public async Task StartAsync_ShouldCreateTableWithCorrectColumns()
        {
            var result = await _fixture.ExecuteScalarAsync(
                FullProgramModel.ManagementConnectionString,
                @"SELECT COUNT(*) FROM information_schema.columns 
                  WHERE table_name = 'management_items';"
            );

            Assert.Equal(5L, Convert.ToInt64(result));
        }
    }
}
