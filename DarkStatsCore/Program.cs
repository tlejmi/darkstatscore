﻿using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using DarkStatsCore.Data;

namespace DarkStatsCore
{
    public class Program
    {
        private static string _listenUrl = "http://*:6677";
        
        public static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                _listenUrl = args[0];
                Console.WriteLine("Using listen address: " + _listenUrl);
            }
            using (var context = new DarkStatsDbContext())
            {
                context.Database.Migrate();
                var settings = new SettingsLib(context);
                Console.WriteLine("Starting DarkStatsCore...");
                if (!string.IsNullOrEmpty(settings.VersionInformation))
                {
                    Console.WriteLine(settings.VersionInformation);
                }
                if (!settings.InvalidSettings)
                {
                    Scraper.StartScrapeTask(settings.SaveTime, settings.Url);
                }
            }
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseUrls(_listenUrl)
                   .UseStartup<Startup>()
                   .Build();        
    }
}
