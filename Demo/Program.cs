using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            string productName = "Mighty Product";

            ProductQuery query = new ProductQuery();

            IEnumerable<Product> matchedProducts = query.Filter(new ProductNameMatchesFilter(productName)).Execute();

            foreach(var product in matchedProducts)
            {
                Console.WriteLine($"Product Id: {product.Id}");
                Console.WriteLine($"Product Name:  {product.Name}");
                Console.WriteLine("Product Tags: ");
                foreach(string tag in product.Tags) Console.WriteLine($"\t{tag}");
                Console.WriteLine();
            }
        }
    }
}
