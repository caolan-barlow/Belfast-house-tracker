using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.Playwright;

// ---------- 1. Download the latest spreadsheet ----------
using var playwright = await Playwright.CreateAsync();

await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false, Channel = "msedge" });

var page = await browser.NewPageAsync();

await page.GotoAsync("https://www.finance-ni.gov.uk/publications/ni-house-price-index-statistical-reports");

var download = await page.RunAndWaitForDownloadAsync(async () =>
{
    await page.GetByRole(AriaRole.Link, new() { NameRegex = new Regex("All NI House Price Index statistics") }).ClickAsync();
});

Directory.CreateDirectory("data");
var filePath = Path.Combine("data", download.SuggestedFilename);
await download.SaveAsAsync(filePath);

Console.WriteLine("Downloaded " + Path.GetFullPath(filePath));

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

// ---------- 3. Print the latest results ----------
Console.WriteLine($"Read {prices.Count} quarters for Belfast");
foreach (var p in prices.TakeLast(5))
{
    Console.WriteLine($"{p.Quarter}: £{p.Price:N0}");
}