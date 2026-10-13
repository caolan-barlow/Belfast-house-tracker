using Microsoft.Playwright;

using var playwright = await Playwright.CreateAsync();

await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false, Channel = "msedge" });

var page = await browser.NewPageAsync();

await page.GotoAsync("https://www.propertypal.com/property-for-sale/belfast");

await page.ScreenshotAsync(new() { Path = "propertypal.png", FullPage = true });

Console.WriteLine("Screenshot saved to " + Path.GetFullPath("propertypal.png"));