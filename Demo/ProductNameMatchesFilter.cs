using DataAccess.Sql.Querying;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    class ProductNameMatchesFilter : QueryFilter
    {
        public string Name { get; set; }

        public ProductNameMatchesFilter() { UsesParameter = true; }

        public ProductNameMatchesFilter(string name) : this() => Name = name;

        public override DbParameter[] GetParameters()
        {
            return new DbParameter[] { DbConfiguration.GetProviderConfiguration().CreateInputParameter("@Name", Name) };
        }

        public override string ToSqlClause() => "Name Like %@Name% OR Description Like %@Name%";
    }
}
