using ClosedXML.Excel;
using HouseTracker.Analysis;
using HouseTracker.Data;
using HouseTracker.Scraping;
using Microsoft.Playwright;
using System.Text.RegularExpressions;


// ---------- 1. Download the latest spreadsheet ----------
var downloader = new HpiDownloader();
var filePath = await downloader.GetLatestSpreadsheetAsync();

// ---------- 2. Read Belfast prices from Table 5 ----------
var reader = new HpiReader();
var prices = reader.ReadBelfastPrices(filePath);

// ---------- 3. Print each quarter with its change from the previous one ----------
var analyser = new PriceAnalyser();
var changes = analyser.CalculateChanges(prices);

Console.WriteLine($"Read {prices.Count} quarters for Belfast");
Console.WriteLine();
Console.WriteLine($"{"Quarter",-10}{"Price",12}{"Change",12}{"Change %",10}");

foreach (var row in changes)
{
    if (row.Change is null)
    {
        Console.WriteLine($"{row.Quarter,-10}{row.Price,12:£#,0}{"-",12}{"-",10}");
    }
    else
    {
        Console.WriteLine($"{row.Quarter,-10}{row.Price,12:£#,0}{row.Change,12:+£#,0;-£#,0;£0}{row.ChangePercent,9:+0.0;-0.0;0.0}%");
    }
}

var summary = analyser.Analyse(prices, 250_000);

// ---------- 4. Print the summary ----------
Console.WriteLine("-------------------------------------------------------------------------------------------------------------------");
Console.WriteLine($"--- Belfast summary ({summary.Latest.Quarter}) ---");
Console.WriteLine("Latest price: " + summary.Latest.Price.ToString("£#,0"));
Console.WriteLine("vs last quarter: " + summary.QuarterChange.ToString("+£#,0;-£#,0;£0") + " (" + summary.QuarterPercent.ToString("+0.0;-0.0;0.0") + "%)");
Console.WriteLine("vs last year: " + summary.YearChange.ToString("+£#,0;-£#,0;£0") + " (" + summary.YearPercent.ToString("+0.0;-0.0;0.0") + "%)");

if (summary.IsRising)
{
    Console.WriteLine($"Trend: Rising for {summary.Streak} quarters in a row");
}
else
{
    Console.WriteLine($"Trend: Falling for {summary.Streak} quarters in a row");
}

if (summary.Gap >= 0)
{
    Console.WriteLine($"Budget: {summary.Gap:£#,0} under your {summary.Budget:£#,0} limit");
}
else
{
    Console.WriteLine($"Budget: {-summary.Gap:£#,0} over your {summary.Budget:£#,0} limit");
}


