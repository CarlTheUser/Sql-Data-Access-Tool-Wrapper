﻿using DataAccess.Sql.Mapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataAccess.Sql
{
    public class SqlCaller
    {
        private readonly ISqlProvider sqlProvider;

        public SqlCaller(ISqlProvider sqlProvider)
        {
            this.sqlProvider = sqlProvider ?? throw new ArgumentNullException("sqlProvider");
        }

        public DataTable Query(DbCommand command)
        {
            DataTable dt = null;

            using (DbConnection connection = sqlProvider.CreateConnection())
            {
                command.Connection = connection;
                try
                {
                    connection.Open();
                    using (DbDataReader dr = command.ExecuteReader())
                    {
                        dt = new DataTable();
                        dt.Load(dr);
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            return dt;
        }

        public DataTable Query(string queryString)
        {
            return Query(sqlProvider.CreateCommand(queryString));
        }

        //Useful if you want to get the underlying schema of a table (Select top 1 * from foo...)
        public DataTable GetSchema(string queryString)
        {
            DataTable dt = null;

            using (DbConnection connection = sqlProvider.CreateConnection())
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = queryString;
                    try
                    {
                        connection.Open();
                        using (DbDataReader dr = command.ExecuteReader())
                        {
                            dt = dr.GetSchemaTable();
                        }
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
            return dt;
        }

        public int ExecuteNonQuery(DbCommand command)
        {

            int ret = 0;
            using (DbConnection connection = sqlProvider.CreateConnection())
            {
                command.Connection = connection;
                try
                {
                    connection.Open();
                    ret = command.ExecuteNonQuery();
                }
                finally
                {
                    connection.Close();
                }
            }
            return ret;
        }

        public int ExecuteNonQuery(string commandString)
        {

            return ExecuteNonQuery(sqlProvider.CreateCommand(commandString));
        }

        public object ExecuteScalar(DbCommand command)
        {
            object obj = null;

            using (DbConnection connection = sqlProvider.CreateConnection())
            {
                command.Connection = connection;
                try
                {
                    connection.Open();
                    obj = command.ExecuteScalar();
                }
                finally
                {
                    connection.Close();
                }
            }
            return obj;
        }

        public object ExecuteScalar(string queryString)
        {
            return ExecuteScalar(sqlProvider.CreateCommand(queryString));
        }
        
        public bool ExecuteTransaction(IEnumerable<Action<DbCommand>> commandActions, Func<Exception, bool> ErrorHandle = null)
        {
            if (commandActions.Count() == 0) return true;
            bool success = false;
            using (DbConnection connection = sqlProvider.CreateConnection())
            {
                DbCommand command = connection.CreateCommand();
                DbTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                    command.Transaction = transaction;
                    foreach (Action<DbCommand> commandAction in commandActions)
                    {
                        commandAction.Invoke(command);
                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    transaction.Commit();
                    success = true;
                }
                catch (Exception e)
                {
                    bool errorHandled = ErrorHandle != null ? ErrorHandle.Invoke(e) : false;

                    if (!errorHandled)
                    {
                        success = false;
                        transaction?.Rollback();
                        throw;
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            return success;
        }

        //Useful for bulk inserts, updates, delete on a single data type or table
        /// <summary>
        /// Wraps identical database operation for sets of T in a transaction.
        /// </summary>
        /// <typeparam name="T">Type of collection</typeparam>
        /// <param name="collection">Collection</param>
        /// <param name="commandInitializer">Initializer for command: Set your command string or command type here.</param>
        /// <param name="bindingAction">Bind your T to command parameters here</param>
        /// <paramref name="ErrorHandle">Optional function that takes exception as argument. Returns true if exception is handled and stops the exception propagation otherwise throws the exception.</paramref>
        /// <returns>Boolean indicating success of the operation</returns>
        public bool OperateCollection<T>(IEnumerable<T> collection, Action<DbCommand> commandInitializer, Action<DbCommand, T> bindingAction, Func<Exception, bool> ErrorHandle = null)
        {
            bool successful = false;

            DbConnection connection = sqlProvider.CreateConnection();
            DbTransaction transaction = null;
            DbCommand command = connection.CreateCommand();

            T[] copy = collection.ToArray();

            int count = copy.Length;

            try
            {
                connection.Open();

                transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                command.Transaction = transaction;

                commandInitializer.Invoke(command);

                for (int i = 0; i != count; ++i)
                {
                    bindingAction.Invoke(command, copy[i]);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }

                transaction.Commit();
                successful = true;
            }
            catch(Exception e)
            {
                bool errorHandled = ErrorHandle != null ? ErrorHandle.Invoke(e) : false; 

                if(!errorHandled)
                {
                    transaction.Rollback();
                    successful = false;
                    throw;
                }
            }
            finally
            {
                connection.Close();
                command.Dispose();
                connection.Dispose();
            }
            return successful;
        }

        public IEnumerable<T> Get<T>(Func<IDataReader, List<T>> mappingMethod, string query)
        {
            DbCommand command = sqlProvider.CreateCommand(query);

            return Get(mappingMethod, command);
        }

        public IEnumerable<T> Get<T>(Func<IDataReader, List<T>> mappingMethod, DbCommand command)
        {
            List<T> temp;

            if (mappingMethod == null) throw new Exception("Mapping method is null");

            using (DbConnection connection = sqlProvider.CreateConnection())
            {
                command.Connection = connection;

                try
                {
                    command.Connection.Open();

                    //var watch = Stopwatch.StartNew();

                    temp = mappingMethod.Invoke(command.ExecuteReader());

                    //watch.Stop();

                    //Debug.WriteLine($"Elapsed time for Property Mapping (Manual Mapping):  {watch.ElapsedMilliseconds}ms");
                    //Debug.WriteLine("Result set Row Count: " + temp.Count);
                }
                finally
                {
                    command.Connection.Close();
                }
            }

            return temp;
        }

        public IEnumerable<T> Get<T>(IDataMapper<T> dataMapper, DbCommand command) where T : class, new()
        {
            List<T> temp = new List<T>();

            using (DbConnection connection = command.Connection = command.Connection ?? sqlProvider.CreateConnection())
            {
                try
                {
                    connection.Open();

                    IDataReader reader = command.ExecuteReader();

                    var watch = Stopwatch.StartNew();

                    while (reader.Read()) temp.Add(dataMapper.CreateMappedInstance(reader));

                    watch.Stop();

                    Debug.WriteLine($"Elapsed time for Property Mapping ({dataMapper.GetType().Name}):  {watch.ElapsedMilliseconds}ms\nResult set Row Count: " + temp.Count);
                }
                finally { connection.Close(); }
            }

            return temp;
        }

        public IEnumerable<T> Get<T>(IDataMapper<T> dataMapper, string query) where T: class, new()
        {
            return Get(dataMapper, sqlProvider.CreateCommand(query));
        }

        public IEnumerable<T> Get<T>(DbCommand command) where T : class, new()
        {
            return Get(new ReflectionDataMapper<T>(), command);
        }

        public IEnumerable<T> Get<T>(string query) where T : class, new()
        {
            return Get<T>(sqlProvider.CreateCommand(query));
        }
        
        public async Task<IEnumerable<T>> GetAsync<T>(IDataMapper<T> dataMapper, DbCommand command, CancellationToken token) where T: class, new()
        {
            List<T> temp = new List<T>();

            using (DbConnection connection = command.Connection = command.Connection ?? sqlProvider.CreateConnection())
            {
                
                try
                {
                    await connection.OpenAsync(token);

                    using (DbDataReader reader = command.ExecuteReaderAsync(token).Result)
                    {
                        while (await reader.ReadAsync(token)) temp.Add(dataMapper.CreateMappedInstance(reader));
                    }
                }
                finally { connection.Close(); }
            }

            return temp;
        }

        // Testing GetAsync<T> simulating delay in query enough to call Cancel() on cancellation token source 
        private async Task<bool> DelayReader(DbDataReader reader, CancellationToken token)
        {
            await Task.Delay(20);
            return await reader.ReadAsync(token);
        }
        
        public async Task<IEnumerable<T>> GetAsync<T>(IDataMapper<T> dataMapper, string query, CancellationToken token) where T: class, new()
        {
            return await GetAsync(dataMapper, sqlProvider.CreateCommand(query), token);
        }
        
        public async Task<IEnumerable<T>> GetAsync<T>(IDataMapper<T> dataMapper, DbCommand command) where T: class, new()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            return await GetAsync(dataMapper, command, source.Token);
        }

        public async Task<IEnumerable<T>> GetAsync<T>(IDataMapper<T> dataMapper, string query) where T: class, new()
        {
            return await GetAsync(dataMapper, sqlProvider.CreateCommand(query));
        }

        public async Task<IEnumerable<T>> GetAsync<T>(DbCommand command) where T: class, new()
        {
            return await GetAsync(new ReflectionDataMapper<T>(), command);
        }

        public async Task<IEnumerable<T>> GetAsync<T>(string query) where T: class, new()
        {
            return await GetAsync<T>(sqlProvider.CreateCommand(query));
        }

        public async Task<IEnumerable<T>> IterateAsync<T>(IDataMapper<T> dataMapper, Action<T> iteratorAction, DbCommand command, CancellationToken token) where T : class, new()
        {
            List<T> temp = new List<T>();

            using (DbConnection connection = command.Connection = command.Connection ?? sqlProvider.CreateConnection())
            {

                try
                {
                    await connection.OpenAsync(token);

                    using (DbDataReader reader = command.ExecuteReaderAsync(token).Result)
                    {
                        while (await reader.ReadAsync(token))  await Task.Run(() => iteratorAction.Invoke(dataMapper.CreateMappedInstance(reader)), token);
                    }
                }
                finally { connection.Close(); }
            }

            return temp;
        }

        public IEnumerable<dynamic> GetDynamic(string commandString)
        {
            return GetDynamic(sqlProvider.CreateCommand(commandString));
        }

        public IEnumerable<dynamic> GetDynamic(DbCommand command)
        {
            List<dynamic> temp = new List<dynamic>();

            using (DbConnection connection = sqlProvider.CreateConnection())
            {
                command.Connection = connection;

                try
                {
                    command.Connection.Open();

                    IDataReader reader = command.ExecuteReader();

                    var watch = Stopwatch.StartNew();

                    DynamicMapper mapper = new DynamicMapper(reader);

                    while (reader.Read()) temp.Add(mapper.CreateMappedInstance(reader));

                    watch.Stop();

                    Console.WriteLine($"Elapsed time for Property Mapping ({mapper.GetType().Name}):  {watch.ElapsedMilliseconds}ms\nResult set Row Count: " + temp.Count);
                }
                finally
                {
                    command.Connection.Close();
                }
            }

            return temp;
        }

        public async Task<IEnumerable<dynamic>> GetDynamicAsync(DbCommand command, CancellationToken token)
        {
            List<dynamic> temp = new List<dynamic>();

            using (DbConnection connection = sqlProvider.CreateConnection())
            {
                command.Connection = connection;

                try
                {
                    await command.Connection.OpenAsync(token);

                    using (DbDataReader reader = await command.ExecuteReaderAsync(token))
                    {
                        DynamicMapper mapper = new DynamicMapper(reader);

                        while (await reader.ReadAsync()) temp.Add(mapper.CreateMappedInstance(reader));
                    }
                }
                finally
                {
                    command.Connection.Close();
                }
            }

            return temp;
        }
    }
}
