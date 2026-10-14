using HouseTracker.Analysis;
using HouseTracker.Data;
using HouseTracker.Reports;
using HouseTracker.Scraping;

// ---------- 1. Download the latest spreadsheet ----------
var filePath = await new HpiDownloader().GetLatestSpreadsheetAsync();

// ---------- 2. Read Belfast prices from Table 5 ----------
var prices = new HpiReader().ReadBelfastPrices(filePath);

// ---------- 3. Calculate quarterly changes and summary ----------
var analyser = new PriceAnalyser();
var changes = analyser.CalculateChanges(prices);
var summary = analyser.Analyse(prices, 250_000);

// ---------- 4. Print the summary ----------
new SummaryPrinter().Print(changes, summary);
