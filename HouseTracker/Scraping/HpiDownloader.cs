using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HouseTracker.Scraping
{
    internal class HpiDownloader
    {
        public async Task<string> GetLatestSpreadsheetAsync()
        {
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

            return filePath;
        }
    }
}
