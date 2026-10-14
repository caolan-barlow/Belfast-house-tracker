using HouseTracker.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace HouseTracker.Analysis
{
    internal class PriceAnalyser
    {
        public List<QuarterChange> CalculateChanges(List<QuarterPrice> prices)
        {
            var changes = new List<QuarterChange>();

            for (int i = 0; i < prices.Count; i++)
            {
                var current = prices[i];

                if (i == 0)
                {
                    // Nothing to compare the first quarter against
                    changes.Add(new QuarterChange(current.Quarter, current.Price, null, null));
                    continue;
                }

                var previous = prices[i - 1];
                var change = current.Price - previous.Price;
                var changePercent = change / previous.Price * 100;

                changes.Add(new QuarterChange(current.Quarter, current.Price, change, changePercent));
            }

            return changes;
        }

        public PriceSummary Analyse(List<QuarterPrice> prices, double budget)
        {
            var latest = prices[^1];
            var lastQuarter = prices[^2];
            var lastYear = prices[^5];

            var quarterChange = latest.Price - lastQuarter.Price;
            var quarterPercent = quarterChange / lastQuarter.Price * 100;

            var yearChange = latest.Price - lastYear.Price;
            var yearPercent = yearChange / lastYear.Price * 100;
            var gap = budget - latest.Price;

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

            return new PriceSummary(
            latest,
            quarterChange,
            quarterPercent,
            yearChange,
            yearPercent,
            isRising,
            streak,
            budget,
            gap);
        }
    }
}
