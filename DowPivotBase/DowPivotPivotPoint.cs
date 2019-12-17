namespace NinjaTrader.Custom.Indicators.DowPivotBase
{
    public class DowPivotPivotPoint
    {
        public DowPivotHighLowPoint FirstLow { get; set; }
        public DowPivotHighLowPoint SecondLow { get; set; }
        public DowPivotHighLowPoint FirstHigh { get; set; }
        public DowPivotHighLowPoint SecondHigh { get; set; }

        public DowPivotPivotPoint()
        {
            FirstLow = new DowPivotHighLowPoint();
            SecondLow = new DowPivotHighLowPoint();
            FirstHigh = new DowPivotHighLowPoint();
            SecondHigh = new DowPivotHighLowPoint();
        }

        public DowPivotPivotPoint(DowPivotHighLowPoint firstLow, DowPivotHighLowPoint secondLow, DowPivotHighLowPoint firstHigh, DowPivotHighLowPoint secondHigh)
        {
            FirstLow = new DowPivotHighLowPoint(firstLow);
            SecondLow = new DowPivotHighLowPoint(secondLow);
            FirstHigh = new DowPivotHighLowPoint(firstHigh);
            SecondHigh = new DowPivotHighLowPoint(secondHigh);
        }
    }
}
