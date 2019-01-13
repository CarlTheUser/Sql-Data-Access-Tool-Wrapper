using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Sql.Mapping
{
    //Not ready - DO NOT USE
    public class ExpressionTreeDataMapper<T> : IDataMapper<T> where T : class, new()
    {
        private static Func<IDataReader, T> MappingMethod;

        private Func<IDataReader, T> CreateMapper(IDataReader reader)
        {
            var expressions = new List<Expression>();

            var functionParameterExpression = Expression.Parameter(typeof(IDataRecord), "reader"); //(IDataRecord reader) =>

            var variableInAFunctionExpression = Expression.Variable(typeof(T), "instance"); //T instance;

            var instantiationExpression = Expression.New(variableInAFunctionExpression.Type); // new T();

            var assignExpression = Expression.Assign(variableInAFunctionExpression, instantiationExpression); // T instance = new T();

            expressions.Add(assignExpression);

            var indexerInfo = typeof(IDataRecord).GetProperty("Item", new[] { typeof(int) });

            int readerFieldCount = reader.FieldCount;

            var columnNames = Enumerable.Range(0, readerFieldCount)
                                        .Select(i => new { i, name = reader.GetName(i) });

            foreach (var column in columnNames)
            {
                var property = variableInAFunctionExpression.Type.GetProperty(
                    column.name,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (property == null)
                    continue;

                var columnIndexExpression = Expression.Constant(column.i); // [0] or [1] or [2] ... etc 

                var propertyIndexerExpression = Expression.MakeIndex(
                    functionParameterExpression, indexerInfo, new[] { columnIndexExpression }); // reader[0] or reader[1] or reader[2] ... etc

                //ConstantExpression typeParam = Expression.Constant(property.PropertyType, typeof(Type));

                var convertExp = Expression.Convert(propertyIndexerExpression, property.PropertyType); // (TargetProperty Type)reader[0] or (TargetProperty Type)reader[1] or (TargetProperty Type)reader[2] ... etc 

                var propertyExpression = Expression.Property(variableInAFunctionExpression, property); // instance.Property1, instance.Property2, ... etc

                var propertyAssignExpression = Expression.Assign(
                    propertyExpression, convertExp); //instance.Property1 = (property1.PropertyType)reader[0]...

                expressions.Add(propertyAssignExpression);

                //instance.Property1 = (property1.PropertyType)reader[0];
                //instance.Property2 = (property2.PropertyType)reader[1];
                //instance.Property3 = (property3.PropertyType)reader[2];
            }

            expressions.Add(variableInAFunctionExpression);

            //expressions :
            //expressions[0] :  T instance = new T();
            //expressions[1] :  instance.Property1 = (property1.PropertyType)reader[0]
            //expressions[2] :  instance.Property2 = (property2.PropertyType)reader[1]
            //expressions[3] :  instance.Property3 = (property3.PropertyType)reader[2]
            //expressions[4] :  instance

            var functionBodyExpression = Expression.Block(new[] { variableInAFunctionExpression }, expressions);

            //functionBodyExpression :
            //  T instance = new T();
            //  instance.Property1 = (property1.PropertyType)reader[0];
            //  instance.Property2 = (property2.PropertyType)reader[1];
            //  instance.Property3 = (property3.PropertyType)reader[2];
            //  return instance;

            var lambdaExpression = Expression.Lambda<Func<IDataReader, T>>(functionBodyExpression, functionParameterExpression);

            //lambdaExpression :
            //  (reader) =>
            //  {
            //      T instance = new T();
            //      instance.Property1 = (property1.PropertyType)reader[0];
            //      instance.Property2 = (property2.PropertyType)reader[1];
            //      instance.Property3 = (property3.PropertyType)reader[2];
            //      return instance;
            //  }

            return lambdaExpression.Compile();
        }

        public T CreateMappedInstance(IDataReader reader)
        {
            if (MappingMethod == null) MappingMethod = CreateMapper(reader);
            return MappingMethod.Invoke(reader);
        }
    }
}
