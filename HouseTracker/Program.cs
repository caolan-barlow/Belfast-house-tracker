using ClosedXML.Excel;
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
Console.WriteLine($"Read {prices.Count} quarters for Belfast");
Console.WriteLine();
Console.WriteLine($"{"Quarter",-10}{"Price",12}{"Change",12}{"Change %",10}");

for (int i = 0; i < prices.Count; i++)
{
    var current = prices[i];

    if (i == 0)
    {
        // Nothing to compare the first quarter against
        Console.WriteLine($"{current.Quarter,-10}{current.Price,12:£#,0}{"-",12}{"-",10}");
        continue;
    }

    var previous = prices[i - 1];
    var change = current.Price - previous.Price;
    var changePercent = change / previous.Price * 100;

    Console.WriteLine($"{current.Quarter,-10}{current.Price,12:£#,0}{change,12:+£#,0;-£#,0;£0}{changePercent,9:+0.0;-0.0;0.0}%");
    
}


// ---------- 4. Print a summary of the latest quarter and compare it to previous periods ----------
var latest = prices[^1];
var lastQuarter = prices[^2];
var lastYear = prices[^5];

var quarterChange = latest.Price - lastQuarter.Price;
var quarterPercent = quarterChange / lastQuarter.Price * 100;

var yearChange = latest.Price - lastYear.Price;
var yearPercent = yearChange / lastYear.Price * 100;

var budget = 250_000;
var gap = budget - latest.Price;

Console.WriteLine("-------------------------------------------------------------------------------------------------------------------");
Console.WriteLine("");
Console.WriteLine($"--- Belfast summary ({latest.Quarter}) ---");
Console.WriteLine("Latest price: " + latest.Price.ToString("£#,0"));
Console.WriteLine("vs last quarter: " + (latest.Price - lastQuarter.Price).ToString("+£#,0;-£#,0;£0") + " (" + quarterPercent.ToString("+0.0;-0.0;0.0") + "%)");
Console.WriteLine("vs last year: " + (latest.Price - lastYear.Price).ToString("+£#,0;-£#,0;£0") + " (" + yearPercent.ToString("+0.0;-0.0;0.0") + "%)");

bool isRising = prices[^1].Price > prices[^2].Price;
int streak = 0;

for (int i = prices.Count - 1; i > 0; i--)
{
    bool wentUp = prices[i].Price > prices[i - 1].Price;

    if (wentUp == isRising)
    {
        streak++;
    }
    else
    {
        break;
    }
}

if (isRising)
{
    Console.WriteLine($"Trend: Rising for {streak} quarters in a row");
}
else
{
    Console.WriteLine($"Trend: Falling for {streak} quarters in a row");
}
if (gap >= 0)
    Console.WriteLine($"Budget: {gap:£#,0} under your {budget:£#,0} limit");
else
    Console.WriteLine($"Budget: {-gap:£#,0} over your {budget:£#,0} limit");

Console.WriteLine("");