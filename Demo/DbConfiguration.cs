using DataAccess.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    class DbConfiguration
    {
        private static readonly string CONNECTION_STRING = "Server=SomeIP;User={DBUSER};Password={PASSWORD};Database={DATABASE}"; 

        public static ISqlProvider GetProviderConfiguration()
        {
            string connectionString = CONNECTION_STRING
                .Replace("{DBUSER}", "get from xml setting")
                .Replace("{PASSWORD}", "get from xml setting")
                .Replace("{DATABASE}", "myDataSource");
            return SqlProviderFactory.CreateMsSqlProvider(connectionString);
        }
    }
}
