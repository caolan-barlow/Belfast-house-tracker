# Belfast House Price Tracker

This tool downloads the latest [NI House Price Index](https://www.finance-ni.gov.uk/publications/ni-house-price-index-statistical-reports) spreadsheet and pulls out house prices for Belfast. It then reports how prices have changed since the last quarter and the last year, whether they're currently rising or falling, and how far they sit from a set budget.

I built it because I'm looking to buy a house in Belfast and wanted a clear, data-backed view of where prices are heading, using official figures rather than asking prices on property sites.

## What it does

1. Uses Playwright to visit the NI Department of Finance statistics page and find the latest House Price Index spreadsheet
2. Downloads it only if it's a new release
3. Reads Belfast's standardised prices from the spreadsheet with ClosedXML
4. Calculates quarter-on-quarter and year-on-year changes, the current price trend, and the gap to a set budget

## Example output

```
--- Belfast summary (2026 Q2) ---
Latest price: £184,768
vs last quarter: +£3,344 (+1.8%)
vs last year: +£13,162 (+7.7%)
Trend: Rising for 10 quarters in a row
Budget: £65,232 under your £250,000 limit
```

## Running it

**Requirements:** [.NET 10 SDK](https://dotnet.microsoft.com/download) and Microsoft Edge.

1. Clone the repository and move into it:
```
   git clone https://github.com/caolan-barlow/Belfast-house-tracker.git
   cd Belfast-house-tracker
```
2. Build the project:
```
   dotnet build
```
3. Run it from the repository root:
```
   dotnet run --project HouseTracker
```

The spreadsheet is saved to a `data` folder in whichever directory you run the command from, so run it from the repository root to keep the downloads in one place.

## Project structure

| Folder   | Responsibility                             |
|----------|--------------------------------------------|
| Scraping | Finds and downloads the latest spreadsheet |
| Data     | Reads Belfast prices from the spreadsheet  |
| Analysis | Calculates changes, trend and budget gap   |
| Reports  | Prints the results                         |

## Design decisions

- **Official data instead of property sites:** My original plan was to scrape PropertyPal, filtering houses in Belfast by area and budget. During implementation I found the site uses Cloudflare bot protection, which stopped listings from loading and sometimes blocked access entirely. Rather than trying to get around it, I switched to the official NI House Price Index from Land & Property Services. It has no bot protection, and because it's based on actual sale prices, not asking prices, it gives a more accurate picture of where the market is going.

- **Skipping unchanged downloads:** The data is only published once a quarter, so most runs would be downloading a file I already have. The tool reads the filename from the link first and only downloads it if that file isn't already saved, which makes repeat runs faster and avoids requesting the same 1.5 MB file from the government site every time.

- **Reading the filename from the link, not hardcoding it:** The filename includes the quarter, so it changes with every release. Hardcoding it would mean the tool never picks up new data.

- **Finding columns by header text, not by position:** The reader searches for the "Sale Quarter" and "Belfast Standardised Price" headers rather than assuming a fixed column or row. If the government adds council areas or moves columns, the tool still works.

- **Standardised price instead of average sale price:** The spreadsheet's per-council standardised price adjusts for the mix of houses sold each quarter, so a quarter with lots of detached sales doesn't inflate it. A simple average sale price would.

- **Separating each job into its own class:** I started with a single Program.cs that did everything, and once it worked I separated each job into its own class. Scraping, Data, Analysis and Reports each do one thing. The analyser returns results and never prints, so the calculations can be tested on their own and reused for other outputs later.

- **Why Playwright and not a plain HTTP download:** The page is static, so a plain HttpClient request would also work. Part of my goal was to get more familiar with Playwright, and finding the link by its accessible role and text, the way a user would, makes the tool resilient to page layout changes.

## Data source

House price data comes from the [Northern Ireland House Price Index](https://www.finance-ni.gov.uk/publications/ni-house-price-index-statistical-reports), published by Land & Property Services and NISRA.

Contains public sector information licensed under the [Open Government Licence v3.0](https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/).

This is a personal project and is not affiliated with or endorsed by the Department of Finance.