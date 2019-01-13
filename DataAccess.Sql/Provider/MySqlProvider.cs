using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Sql.Provider
{
    internal class MySqlProvider : ISqlProvider
    {
        public string ConnectionString { get; set; }

        public MySqlProvider()
        {

        }

        public MySqlProvider(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public DbConnection CreateConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public DbConnection CreateOpenedConnection()
        {
            DbConnection connection = new MySqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        public DbConnection CreateConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        public DbConnection CreateOpenedConnection(string connectionString)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        public DbCommand CreateCommand(string commandString, DbParameter[] inputParams = null, DbParameter[] outputParams = null)
        {
            MySqlCommand cmd = new MySqlCommand(commandString) { CommandType = CommandType.Text };
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
            MySqlCommand cmd = new MySqlCommand(storedProcedure) { CommandType = CommandType.StoredProcedure };
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
            return new MySqlParameter
            {
                ParameterName = parameterName,
                Value = value,
                Direction = ParameterDirection.Input,
                DbType = dbType
            };
        }

        public DbParameter CreateOutputParameter(string parameterName)
        {
            return new MySqlParameter
            {
                ParameterName = parameterName,
                Direction = ParameterDirection.Output
            };
        }

        public DbParameter CreateReturnParameter()
        {
            return new MySqlParameter
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
                return source.Select(x => new MySqlParameter
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
                return source.Select(x => new MySqlParameter
                {
                    ParameterName = x,
                    Direction = ParameterDirection.Output
                }).ToArray();
            }
        }
    }
}
