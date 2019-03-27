using DataAccess.Sql;
using DataAccess.Sql.Mapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            //string productName = "Mighty Product";

            //ProductQuery query = new ProductQuery();

            //IEnumerable<Product> matchedProducts = query
            //    .Filter(new ProductNameMatchesFilter(productName))
            //    .Execute();

            //foreach (var product in matchedProducts)
            //{
            //    Console.WriteLine($"Product Id: {product.Id}");
            //    Console.WriteLine($"Product Name:  {product.Name}");
            //    Console.WriteLine("Product Tags: ");
            //    foreach (string tag in product.Tags) Console.WriteLine($"\t{tag}");
            //    Console.WriteLine();
            //}

            ISqlProvider provider = SqlProviderFactory.CreateMySqlProvider("Server=localhost;user=root;password=;database=dbbernasorrags;");

            SqlCaller caller = new SqlCaller(provider);

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            CancellationToken token = tokenSource.Token;

            caller.GetAsync(
                new ReflectionDataMapper<UserActivity>(),
                provider.CreateCommand("SELECT LogId, UserId, UserInfo, LogDate, ActivityScope, ActivityDetails FROM vw_useractivitylog"),
                token).ContinueWith(activitiesTask =>
            {
                if (activitiesTask.IsCompleted || activitiesTask.IsCanceled)
                {
                    IEnumerable<UserActivity> activities = activitiesTask.Result;
                    foreach (UserActivity activity in activities)
                    {
                        Console.WriteLine($"{activity.ActivityNumber}\n{activity.Timestamp}\n{activity.UserInfo}\n");
                    }
                    Console.ReadKey();
                }
                else Console.WriteLine("An error occured.");

            }).GetAwaiter().GetResult();

        }

        class UserActivitiesMapper : ReflectionDataMapper<UserActivity>
        {
            CancellationTokenSource Source;

            int Count = 0;

            public UserActivitiesMapper(CancellationTokenSource source)
            {
                Source = source;
            }

            public override UserActivity CreateMappedInstance(IDataReader reader)
            {
                if (Count == 4) Source.Cancel();
                UserActivity activity = base.CreateMappedInstance(reader);
                ++Count;
                return activity;
            }
        }


        class UserActivity
        {
            [DataField("LogID")]
            public int ActivityNumber { get; set; }

            public int UserId { get; set; }

            public string UserInfo { get; set; }

            [DataField("LogDate")]
            public DateTime Timestamp { get; set; }

            [DataField("ActivityScope")]
            public string Scope { get; set; }

            [DataField("ActivityDetails")]
            public string Details { get; set; }

            public UserActivity() { }
        }

    }
}
