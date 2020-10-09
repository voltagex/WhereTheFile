using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using EFCore.BulkExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WhereTheFile.Database;
using WhereTheFile.Types;

namespace WhereTheFile
{
    class Program
    {
        private static string[] drives;
        private static Settings Settings = new Settings();
        private static IFileScanner scanner;

        static void Main(string[] args)
        {
            if (Environment.GetEnvironmentVariable("DEBUG") != null)
            {
                {
                    Console.WriteLine("Waiting for debugger");
                    while (!Debugger.IsAttached)
                    {
                        Thread.Sleep(1000);
                    }
                    Debugger.Break();
                }
            }

            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            Log.Logger = new LoggerConfiguration()
                //.ReadFrom.Configuration(builder.Build())
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information("Starting Application");

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IWTFContext, WTFContext>();
                    services.AddTransient<IFileScanner, WindowsPathScanner>();
                })
                .UseSerilog()
                .Build();
            Log.Logger.Information("Services configured");

            var service = ActivatorUtilities.CreateInstance<ConsoleMenuService>(host.Services);
            service.Menu();
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();
        }
    }
}


