using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Sql
{
    public interface ISqlProvider
    {
        string ConnectionString { get; set; }

        DbConnection CreateConnection();
        DbConnection CreateOpenedConnection();

        DbCommand CreateCommand(string commandString, DbParameter[] inputParams = null, DbParameter[] outputParams = null);
        DbCommand CreateCommandSP(string storedProcedure, DbParameter[] inputParams = null, DbParameter[] outputParams = null);

        DbDataReader CreateReader(IDbCommand command);

        DbParameter CreateInputParameter(string parameterName, object value, DbType dbType = DbType.Object);

        DbParameter CreateOutputParameter(string parameterName);

        DbParameter CreateReturnParameter();

        DbParameter[] CreateInputParameters(Dictionary<string, object> source);
        DbParameter[] CreateOutputParameters(string[] source);
    }
}
