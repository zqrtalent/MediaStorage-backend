using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using MediaStorage.Data.Streaming.Context;
using MediaStorage.Data.Media.Context;

using Microsoft.Extensions.DependencyInjection;
using MediaStorage.Common.DI.Extensions;
using MediaStorage.Common.DI;

namespace MediaStorage.Data.ConsoleApp
{
    public interface itest
    {
    }

    public class test : itest
    {
        private readonly MediaDataContext _data;
        public test(MediaDataContext data)
        {
            _data = data;
        }
    }


    class Program
    {
        //public static IAbstractDependencyInjector _container;
        public static IConfigurationRoot Configuration { get; set; }

        static void Main(string[] args)
        {
            UnitTest11.TestMethod1();
            
            // Build configuration.
            LoadConfiguration();

//          var services = new ServiceCollection();
//          services.AddDbContext<MediaDataContext>(options => options.UseSqlite("Data Source=blog.db"));
            //services.AddDbContext<MediaDataContext>(options => options.UseMySql(Configuration.GetConnectionString("RemoteMySqlConnection")));

            //_container = MicrosoftDependencyExtensions.UseMicrosoftDI();
            
            using(var container = MicrosoftDependencyExtensions.UseMicrosoftDI())
            {
                //services.AddDbContext<MediaDataContext>(options => options.UseMySql(Configuration.GetConnectionString("RemoteMySqlConnection")));
                //coll.AddDbContext<MediaDataContext>();

                // Register services.
                RegisterServices(container);

                var config = container.Resolve<IConfiguration>();
                var config1 = container.Resolve<IConfiguration>();

                if(config != config)
                {
                    Console.WriteLine("Error singleton resolve!");
                }

                // Not being desposed automatically !!!
                //var dbContext1 = container.Resolve<IMediaDataContext>();

                var t = container.Resolve<itest>();
                Console.WriteLine("====");


                // DataContext test.
                // using(var dbContext = container.Resolve<IMediaDataContext>())
                // {
                // }
            }
        }

        static void LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
        }

        static void RegisterServices(IAbstractDependencyInjector di)
        {
            // Microsoft.Extensions.Configuration.IConfigurationRoot
            // Microsoft.Extensions.Configuration.IConfiguration
            //var ss = typeof(Configuration).Name;

            // Add services here.
            di.AddSingletone(Configuration);
            //di.AddPerThread<IMediaDataContext, MediaDataContext>();
            di.AddPerThread<itest, test>();

            di.Build();
        }
    }
}
