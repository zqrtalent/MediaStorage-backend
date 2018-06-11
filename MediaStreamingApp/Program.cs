using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;

namespace MediaStreamingApp
{
    public class Program
    {
/* 
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                // .UseKestrel(options => {
                //     options.AddServerHeader = false;
                //     options.Listen(System.Net.IPAddress.Any, 5001, listenOptions => {
                //     });
                // })  
                .UseKestrel()  
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();
            host.Run();
        }*/

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options => {
                    options.Listen(IPAddress.Any, 5000);
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();
    }
}