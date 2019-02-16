using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Sql.Querying
{
    // Implement this class for your queries
    public abstract class SqlQuery<T>
    {
        protected QueryFilter _filter;

        public SqlQuery<T> Filter(QueryFilter filter)
        {
            _filter = filter;
            return this;
        }

        public abstract IEnumerable<T> Execute();
    }
}
