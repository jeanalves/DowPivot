namespace NinjaTrader.NinjaScript.Indicators.DowPivotBase
{
    public enum Situation
    {
        AddLow,
        AddHigh,
        UpdateLow,
        UpdateHigh
    };
    public enum TrendDir
    {
        Up,
        Down,
        Unknown
    };
    public enum ZigZagCalculationType
    {
        Points,
        Swing
    }
}
