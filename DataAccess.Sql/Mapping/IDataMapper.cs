using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Sql.Mapping
{
    public interface IDataMapper<T> where T : class, new()
    {
        T CreateMappedInstance(IDataReader reader);
    }
}
