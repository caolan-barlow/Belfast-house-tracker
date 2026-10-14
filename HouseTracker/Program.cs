using System.Text.RegularExpressions;
using Microsoft.Playwright;

using var playwright = await Playwright.CreateAsync();

await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false, Channel = "msedge" });

var page = await browser.NewPageAsync();

await page.GotoAsync("https://www.finance-ni.gov.uk/publications/ni-house-price-index-statistical-reports");

// Click the spreadsheet link and capture the file it triggers
var download = await page.RunAndWaitForDownloadAsync(async () =>
{
    await page.GetByRole(AriaRole.Link, new() { NameRegex = new Regex("All NI House Price Index statistics") }).ClickAsync();
});

Directory.CreateDirectory("data");
var filePath = Path.Combine("data", download.SuggestedFilename);
await download.SaveAsAsync(filePath);

Console.WriteLine("Downloaded " + Path.GetFullPath(filePath));