using DataAccess.Sql.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Sql
{
    public class SqlProviderFactory
    {
        public static ISqlProvider CreateMsSqlProvider(string connectionString)
        {
            return new SqlServerProvider(connectionString);
        }

        public static ISqlProvider CreateMySqlProvider(string connectionString)
        {
            return new MySqlProvider(connectionString);
        }

        public static ISqlProvider CreateOleDbSqlProvider(string connectionString)
        {
            return new OleDbProvider(connectionString);
        }

        public static ISqlProvider CreatePostgreSqlProvider(string connectionString)
        {
            return new PostgreSqlProvider(connectionString);
        }
    }
}
