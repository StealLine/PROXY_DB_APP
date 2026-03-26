using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEST_PROXY.Fixtures;

namespace TEST_PROXY
{
    [CollectionDefinition("PostgresCollection")]
    public class PostgresCollection : ICollectionFixture<PostgresFixture> { }
}
