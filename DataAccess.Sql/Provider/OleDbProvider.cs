using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Sql.Provider
{
    internal class OleDbProvider : ISqlProvider
    {
        public string ConnectionString { get; set; }

        public OleDbProvider()
        {

        }

        public OleDbProvider(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public DbConnection CreateConnection()
        {
            return new OleDbConnection(ConnectionString);
        }

        public DbConnection CreateOpenedConnection()
        {
            DbConnection connection = new OleDbConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        public DbConnection CreateConnection(string connectionString)
        {
            return new OleDbConnection(connectionString);
        }

        public DbConnection CreateOpenedConnection(string connectionString)
        {
            OleDbConnection conn = new OleDbConnection(connectionString);
            conn.Open();
            return conn;
        }

        public DbCommand CreateCommand(string commandString, DbParameter[] inputParams = null, DbParameter[] outputParams = null)
        {
            OleDbCommand cmd = new OleDbCommand(commandString) { CommandType = CommandType.Text };
            if (inputParams != null)
            {
                if (inputParams.Length > 0) cmd.Parameters.AddRange(inputParams);
            }
            if (outputParams != null)
            {
                if (outputParams.Length > 0) cmd.Parameters.AddRange(outputParams);
            }
            return cmd;
        }

        public DbCommand CreateCommandSP(string storedProcedure, DbParameter[] inputParams = null, DbParameter[] outputParams = null)
        {
            OleDbCommand cmd = new OleDbCommand(storedProcedure) { CommandType = CommandType.StoredProcedure };
            if (inputParams != null)
            {
                if (inputParams.Length > 0) cmd.Parameters.AddRange(inputParams);
            }
            if (outputParams != null)
            {
                if (outputParams.Length > 0) cmd.Parameters.AddRange(outputParams);
            }
            return cmd;
        }

        public DbDataReader CreateReader(IDbCommand command)
        {
            return (DbDataReader)command.ExecuteReader();
        }

        public DbParameter CreateInputParameter(string parameterName, object value, DbType dbType = DbType.Object)
        {
            return new OleDbParameter
            {
                ParameterName = parameterName,
                Value = value,
                Direction = ParameterDirection.Input,
                DbType = dbType
            };
        }

        public DbParameter CreateOutputParameter(string parameterName)
        {
            return new OleDbParameter
            {
                ParameterName = parameterName,
                Direction = ParameterDirection.Output
            };
        }

        public DbParameter CreateReturnParameter()
        {
            return new OleDbParameter
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
                return source.Select(x => new OleDbParameter
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
                return source.Select(x => new OleDbParameter
                {
                    ParameterName = x,
                    Direction = ParameterDirection.Output
                }).ToArray();
            }
        }
    }
}
