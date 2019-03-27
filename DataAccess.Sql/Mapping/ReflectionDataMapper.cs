using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Sql.Mapping
{
    public class ReflectionDataMapper<T> : IDataMapper<T> where T : class, new()
    {
        //private static readonly IDictionary<string, PropertyInfo> PropertyMappingsCache;
        private static readonly IDictionary<string, PropertyMap> PropertyMappingsCache;

        static ReflectionDataMapper()
        {
            if (PropertyMappingsCache == null)
            {
                //PropertyMappingsCache = new Dictionary<string, PropertyInfo>();

                //PropertyMappingsCache = new Dictionary<string, PropertyMap>();

                StringComparer comparer = StringComparer.OrdinalIgnoreCase;
                PropertyMappingsCache = new Dictionary<string, PropertyMap>(comparer);
                
                Type type = typeof(T);

                PropertyInfo[] properties = type.GetProperties();

                foreach (PropertyInfo prop in properties)
                {
                    var attributes = prop.GetCustomAttributes(false);

                    DataFieldAttribute columnMapping = (DataFieldAttribute)attributes.FirstOrDefault(a => a.GetType() == typeof(DataFieldAttribute));

                    if (columnMapping != null)
                    {
                        string columnName = columnMapping.Column;
                        PropertyMappingsCache.Add(columnName, new PropertyMap(prop, columnName, columnMapping.FieldMapperType));
                    }
                    else PropertyMappingsCache.Add(prop.Name, new PropertyMap(prop));
                }
            }
        }

        public virtual T CreateMappedInstance(IDataReader reader)
        {
            var mappingsCopy = PropertyMappingsCache;

            int readerColumnCount = reader.FieldCount;

            T item = new T();

            for (int i = 0; i < readerColumnCount; ++i)
            {
                if (mappingsCopy.TryGetValue(reader.GetName(i), out PropertyMap propertyMap))
                {
                    object value = reader[i];
                    PropertyInfo property = propertyMap.PropertyInfo;
                    FieldMapping customMapping = propertyMap.CustomMapping;
                    if (value != DBNull.Value) property.SetValue(item, !propertyMap.HasCustomMapping ? value : customMapping.Map(value), null);
                }
            }

            return item;
        }

        private class PropertyMap
        {
            public PropertyInfo PropertyInfo { get; set; }
            public string Column { get; set; }
            //public Func<object, object> CustomMapping { get; }
            public FieldMapping CustomMapping { get; }

            public bool HasCustomMapping { get; }

            public PropertyMap(PropertyInfo prop) : this(prop, prop.Name) { }

            public PropertyMap(PropertyInfo prop, string column) : this(prop, column, null) { }

            public PropertyMap(PropertyInfo prop, string column, Type customMapperType)
            {
                PropertyInfo = prop;
                Column = column;
                HasCustomMapping = (CustomMapping = customMapperType != null ? (FieldMapping)Activator.CreateInstance(customMapperType) : null) != null;
            }

        }
    }
}
