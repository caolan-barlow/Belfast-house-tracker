using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HouseTracker.Data
{
    internal class HpiReader
    {
        public List<QuarterPrice> ReadBelfastPrices(string filePath)
        {
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
            var prices = new List<QuarterPrice>();
            string year = "";
            for (int row = headerRow + 1; !sheet.Cell(row, quarterCol).IsEmpty(); row++)
            {
                var yearText = sheet.Cell(row, yearCol).GetString();
                if (yearText != "") year = yearText;   // carry the year forward for Q2–Q4

                var quarter = sheet.Cell(row, quarterCol).GetString();
                var price = sheet.Cell(row, priceCol).GetDouble();

                prices.Add(new QuarterPrice($"{year} {quarter}", price));
            }

            return prices;
        }

    }
}
