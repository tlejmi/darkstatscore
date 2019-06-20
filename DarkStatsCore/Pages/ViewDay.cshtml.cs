using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DarkStatsCore.Data;
using DarkStatsCore.Models;
using StackExchange.Profiling;

namespace DarkStatsCore.Pages
{
    public class ViewDayModel : PageModel
    {
        private readonly DarkStatsDbContext _context;
        public IEnumerable<DayDataModel> DayData = new List<DayDataModel>();
        public bool DaySelected = false;
        public DateTime MinDay => _context.TrafficStats.Min(t => t.Day);
        public DateTime MaxDay => _context.TrafficStats.Max(t => t.Day);

        public ViewDayModel(DarkStatsDbContext darkStatsDbContext)
        {
            _context = darkStatsDbContext;
            _context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
        }

        public void OnGet(DateTime day)
        {
            if (day != DateTime.MinValue)
            {
                DaySelected = true;
                DayData = GetDayData(day.Month, day.Year, day.Day);
            }
        }

        private IEnumerable<DayDataModel> GetDayData(int month, int year, int day)
        {
            using (MiniProfiler.Current.Step("GetDayData"))
            {
                return _context.TrafficStats
                            .Where(t => t.Day.Year == year && t.Day.Month == month && t.Day.Day == day)
                            .GroupBy(t => t.Day)
                            .Select(t => new DayDataModel
                            {
                                Hour = t.Key,
                                TotalBytes = t.Sum(c => c.In + c.Out).BytesToString(),
                                GraphBytesIn = t.Sum(c => c.In),
                                GraphBytesOut = t.Sum(c => c.Out),
                                TopConsumers = t.OrderByDescending(c => c.Out + c.In)
                                                .Select(c => new TrafficStatsModel
                                                {
                                                    Hostname = (string.IsNullOrEmpty(c.Hostname) || c.Hostname == "(none)") ? c.Ip : c.Hostname,
                                                    Total = (c.Out + c.In).BytesToString()
                                                }).Take(3)
                            });
            }
        }
    }
}
