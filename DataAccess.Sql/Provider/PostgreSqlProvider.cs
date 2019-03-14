using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Sql.Provider
{
    internal class PostgreSqlProvider : ISqlProvider
    {
        public string ConnectionString { get; set; }

        public PostgreSqlProvider() { }

        public PostgreSqlProvider(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public DbConnection CreateConnection()
        {
            return new NpgsqlConnection(ConnectionString);
        }

        public DbConnection CreateOpenedConnection()
        {
            DbConnection connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        public DbConnection CreateConnection(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }

        public DbConnection CreateOpenedConnection(string connectionString)
        {
            NpgsqlConnection conn = new NpgsqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        public DbCommand CreateCommand(string commandString, DbParameter[] inputParams = null, DbParameter[] outputParams = null)
        {
            NpgsqlCommand cmd = new NpgsqlCommand(commandString) { CommandType = CommandType.Text };
            if (inputParams != null && inputParams.Length > 0) cmd.Parameters.AddRange(inputParams);
            if (outputParams != null && outputParams.Length > 0) cmd.Parameters.AddRange(outputParams);
            return cmd;
        }

        public DbCommand CreateCommandSP(string storedProcedure, DbParameter[] inputParams = null, DbParameter[] outputParams = null)
        {
            NpgsqlCommand cmd = new NpgsqlCommand(storedProcedure) { CommandType = CommandType.StoredProcedure };
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
            return new NpgsqlParameter
            {
                ParameterName = parameterName,
                Value = value,
                Direction = ParameterDirection.Input,
                DbType = dbType
            };
        }

        public DbParameter CreateOutputParameter(string parameterName)
        {
            return new NpgsqlParameter
            {
                ParameterName = parameterName,
                Direction = ParameterDirection.Output
            };
        }

        public DbParameter CreateReturnParameter()
        {
            return new NpgsqlParameter
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
                return source.Select(x => new NpgsqlParameter
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
                return source.Select(x => new NpgsqlParameter
                {
                    ParameterName = x,
                    Direction = ParameterDirection.Output
                }).ToArray();
            }
        }
    }
}
