namespace NinjaTrader.Custom.Indicators.DowPivotBase
{
    public class PivotPoint
    {
        public HighLowPoint FirstLow { get; set; }
        public HighLowPoint SecondLow { get; set; }
        public HighLowPoint FirstHigh { get; set; }
        public HighLowPoint SecondHigh { get; set; }

        public PivotPoint()
        {
            FirstLow = new HighLowPoint();
            SecondLow = new HighLowPoint();
            FirstHigh = new HighLowPoint();
            SecondHigh = new HighLowPoint();
        }

        public PivotPoint(HighLowPoint firstLow, HighLowPoint secondLow, HighLowPoint firstHigh, HighLowPoint secondHigh)
        {
            FirstLow = new HighLowPoint(firstLow);
            SecondLow = new HighLowPoint(secondLow);
            FirstHigh = new HighLowPoint(firstHigh);
            SecondHigh = new HighLowPoint(secondHigh);
        }
    }
}
