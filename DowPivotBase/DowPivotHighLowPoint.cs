using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
using System.Windows.Media;

namespace NinjaTrader.Custom.Indicators.DowPivotBase
{
    public class DowPivotHighLowPoint
    {

        public double Price { get; private set; }
        public int BarIndex { get; private set; }
        public int PointIndex { get; private set; }
        private readonly TrendDir trendDir;

        #region Constructors
        public DowPivotHighLowPoint()
        {
            trendDir = TrendDir.Unknown;
        }

        public DowPivotHighLowPoint(DowPivotHighLowPoint hlp)
        {
            Price = hlp.Price;
            BarIndex = hlp.BarIndex;
            trendDir = TrendDir.Unknown;
        }

        public DowPivotHighLowPoint(DowPivot dowPivot, double price, int barIndex, int pointIndex, TrendDir trendDir)
        {
            this.Price = price;
            this.BarIndex = barIndex;
            this.PointIndex = pointIndex;
            this.trendDir = trendDir;
            PrintPoint(dowPivot);
        }
        #endregion

        public void Update(DowPivot dowPivot, double price, int barIndex)
        {
            this.Price = price;
            this.BarIndex = barIndex;
            PrintPoint(dowPivot);
        }
        private void PrintPoint(DowPivot dowPivot)
        {
            if (!dowPivot.DrawProp.ShowTopBottomPoints)
                return;
            switch (trendDir)
            {
                case TrendDir.Down:
                    Draw.Dot(dowPivot, (trendDir + " Dot " + PointIndex.ToString()), false,
                                DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, BarIndex), Price, Brushes.Red).OutlineBrush = Brushes.Transparent;
                    break;
                case TrendDir.Up:
                    Draw.Dot(dowPivot, (trendDir + " Dot " + PointIndex.ToString()), false,
                                DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, BarIndex), Price, Brushes.Green).OutlineBrush = Brushes.Transparent;
                    break;
            }

        }
    }
}
