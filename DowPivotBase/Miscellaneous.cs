using NinjaTrader.NinjaScript.Indicators;

namespace NinjaTrader.Custom.Indicators.DowPivotBase
{
    public static class Miscellaneous
    {
        public static double GetCloseOrLow(DowPivot dowPivot)
        {
            return dowPivot.UseHighLow ? dowPivot.Low[0] : dowPivot.Close[0];
        }
        public static double GetClosrOrHigh(DowPivot dowPivot)
        {
            return dowPivot.UseHighLow ? dowPivot.High[0] : dowPivot.Close[0];
        }

        // Convert number index bar to bars ago
        public static int ConvertBarIndexToBarsAgo(DowPivot dowPivot, int barIndex)
        {
            return ((barIndex - dowPivot.CurrentBar) < 0) ? ((barIndex - dowPivot.CurrentBar) * -1) : (barIndex - dowPivot.CurrentBar);
        }
    }
}
