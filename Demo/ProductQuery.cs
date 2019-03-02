using DataAccess.Sql;
using DataAccess.Sql.Mapping;
using DataAccess.Sql.Querying;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    class ProductQuery : SqlQuery<Product>
    {
        /* Query:
         SELECT 
            P.Id, 
            P.Brand, 
            P.Name, 
            P.Price,
	        (SELECT PT.Name + ',' AS AS[text()]      //Select all taggings in one query 
	        FROM Product_Taggings PT                 //and puts them in a single column separated by ','
	        WHERE PT.ProductId = P.Id                // tag1,tag2,beverages,beer,etc_tag  
	        FOR XML PATH('')          
	        ) AS Taggings
         FROM Products P
        */

        private static readonly string BASE_QUERY = "SELECT P.Id, P.Brand, P.Name, P.Price, (SELECT PT.Name + ', ' AS AS[text()] FROM Product_Taggings PT WHERE PT.ProductId = P.Id FOR XML PATH('') ) AS Taggings FROM Products P ";

        private readonly ISqlProvider provider;

        private readonly SqlCaller caller;

        public ProductQuery()
        {
            caller = new SqlCaller(provider = DbConfiguration.GetProviderConfiguration());
        }

        public override IEnumerable<Product> Execute()
        {
            string query = BASE_QUERY;

            bool usesParameter = false;

            DbParameter[] parameters = null;

            if (_filter != null)
            {
                usesParameter = _filter.UsesParameter;

                query += $"WHERE {_filter.ToSqlClause()} ";

                parameters = _filter.GetParameters();
            }

            DbCommand command = usesParameter ? provider.CreateCommand(query, parameters) : provider.CreateCommand(query);

            IEnumerable<Product>  products = caller.Get(new ReflectionDataMapper<Product>(), command);

            return products;
        }
    }
}
