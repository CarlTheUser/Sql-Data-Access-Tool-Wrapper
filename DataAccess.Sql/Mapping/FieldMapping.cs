using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Sql.Mapping
{
    //Useful for custom mapping of fields 
    // - flattened querries that returns xml/json or comma separated values and maps to object arrays (String[], Tag[] -> collection of { id: name: } pairs), etc
    // - Different Datatypes sql 'Yes'/'No' to bool (for whatever reason ur db structure is like that)

    public abstract class FieldMapping
    {
        public FieldMapping() { }

        public abstract object Map(object source);
    }
}
