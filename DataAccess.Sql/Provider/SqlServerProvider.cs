using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Sql.Provider
{
    internal class SqlServerProvider : ISqlProvider
    {
        public string ConnectionString { get; set; }

        public SqlServerProvider()
        {

        }

        public SqlServerProvider(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public DbConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public DbConnection CreateOpenedConnection()
        {
            DbConnection connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        public DbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public DbConnection CreateOpenedConnection(string connectionString)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        public DbCommand CreateCommand(string commandString, DbParameter[] inputParams = null, DbParameter[] outputParams = null)
        {
            SqlCommand cmd = new SqlCommand(commandString) { CommandType = CommandType.Text };
            if (inputParams != null && inputParams.Length > 0) cmd.Parameters.AddRange(inputParams);
            if (outputParams != null && outputParams.Length > 0) cmd.Parameters.AddRange(outputParams);
            return cmd;
        }

        public DbCommand CreateCommandSP(string storedProcedure, DbParameter[] inputParams = null, DbParameter[] outputParams = null)
        {
            SqlCommand cmd = new SqlCommand(storedProcedure) { CommandType = CommandType.StoredProcedure };
            if (inputParams != null && inputParams.Length > 0) cmd.Parameters.AddRange(inputParams);
            if (outputParams != null && outputParams.Length > 0) cmd.Parameters.AddRange(outputParams);
            return cmd;
        }

        public DbDataReader CreateReader(IDbCommand command)
        {
            return (DbDataReader)command.ExecuteReader();
        }

        public DbParameter CreateInputParameter(string parameterName, object value, DbType dbType = DbType.Object)
        {
            return new SqlParameter
            {
                ParameterName = parameterName,
                Value = value,
                Direction = ParameterDirection.Input,
                DbType = dbType
            };
        }

        public DbParameter CreateOutputParameter(string parameterName)
        {
            return new SqlParameter
            {
                ParameterName = parameterName,
                Direction = ParameterDirection.Output
            };
        }

        public DbParameter CreateReturnParameter()
        {
            return new SqlParameter
            {
                Direction = ParameterDirection.ReturnValue
            };
        }

        public DbParameter[] CreateInputParameters(Dictionary<string, object> source)
        {
            if (source == null)
            {
                return null;
            }
            else
            {
                return source.Select(x => new SqlParameter
                {
                    ParameterName = x.Key,
                    Value = x.Value,
                    Direction = ParameterDirection.Input
                }).ToArray();
            }
        }

        public DbParameter[] CreateOutputParameters(string[] source)
        {
            if (source == null) return null;
            else
            {
                return source.Select(x => new SqlParameter
                {
                    ParameterName = x,
                    Direction = ParameterDirection.Output
                }).ToArray();
            }
        }
    }
}
