using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.Playwright;

// ---------- 1. Download the latest spreadsheet ----------
using var playwright = await Playwright.CreateAsync();

await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false, Channel = "msedge" });

var page = await browser.NewPageAsync();

await page.GotoAsync("https://www.finance-ni.gov.uk/publications/ni-house-price-index-statistical-reports");

// Find the spreadsheet link once and reuse it
var spreadsheetLink = page.GetByRole(AriaRole.Link, new() { NameRegex = new Regex("All NI House Price Index statistics") });

// Read the file's URL from the link, then take the filename from the end of it
var href = await spreadsheetLink.GetAttributeAsync("href");
var fileName = Uri.UnescapeDataString(Path.GetFileName(href!));

Console.WriteLine("Latest file on site: " + fileName);

Directory.CreateDirectory("data");
var filePath = Path.Combine("data", fileName);

if (!File.Exists(filePath))
{
    var download = await page.RunAndWaitForDownloadAsync(async () =>
    {
        await spreadsheetLink.ClickAsync();
    });

    await download.SaveAsAsync(filePath);
    Console.WriteLine("Downloaded new file: " + fileName);
}
else
{
    Console.WriteLine("Already have the latest file, skipping download");
}

// ---------- 2. Read Belfast prices from Table 5 ----------
using var workbook = new XLWorkbook(filePath);
var sheet = workbook.Worksheet("Table 5");

// Header text wraps across lines in Excel, so collapse whitespace before comparing
string Clean(string text) => Regex.Replace(text, @"\s+", " ").Trim();

// Find the "Sale Quarter" header; the year column is the one to its left
var quarterHeader = sheet.CellsUsed().First(c => Clean(c.GetString()) == "Sale Quarter");
int headerRow = quarterHeader.Address.RowNumber;
int quarterCol = quarterHeader.Address.ColumnNumber;
int yearCol = quarterCol - 1;

// Find Belfast's price column on the same header row
int priceCol = sheet.Row(headerRow).CellsUsed()
    .First(c => Clean(c.GetString()) == "Belfast Standardised Price")
    .Address.ColumnNumber;

// Walk down the rows until the quarter column runs out
var prices = new List<(string Quarter, double Price)>();
string year = "";
for (int row = headerRow + 1; !sheet.Cell(row, quarterCol).IsEmpty(); row++)
{
    var yearText = sheet.Cell(row, yearCol).GetString();
    if (yearText != "") year = yearText;   // carry the year forward for Q2–Q4

    var quarter = sheet.Cell(row, quarterCol).GetString();
    var price = sheet.Cell(row, priceCol).GetDouble();

    prices.Add(($"{year} {quarter}", price));
}

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