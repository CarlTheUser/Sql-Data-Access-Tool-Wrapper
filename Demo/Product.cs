using DataAccess.Sql.Mapping;
using System;
using System.Linq;
namespace Demo
{
    //Simple Data Access Class for Product (this is not Domain Model Object) for persistence layer only.
    class Product
    {
        public int Id { get; set; }

        public string Brand { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }

        //Handles different names in query result and data type conversion
        [DataField("Taggings", typeof(ProductTagsMapping))]
        public string[] Tags { get; set; }
    }

    public class ProductTagsMapping : FieldMapping
    {
        public override object Map(object source)
        {
            string tagsString = (string)source;
            return tagsString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(tag => tag).ToArray();
        }
    }
}
