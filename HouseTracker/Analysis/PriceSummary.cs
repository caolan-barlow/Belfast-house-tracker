using HouseTracker.Data;

namespace HouseTracker.Analysis;

public record PriceSummary(
    QuarterPrice Latest,
    double QuarterChange,
    double QuarterPercent,
    double YearChange,
    double YearPercent,
    bool IsRising,
    int Streak,
    double Budget,
    double Gap);