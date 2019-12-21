﻿using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
using System.Windows.Media;

namespace NinjaTrader.Custom.Indicators.DowPivotBase
{
    public class HighLowPoint
    {

        public double Price { get; private set; }
        public int BarIndex { get; private set; }
        public int PointIndex { get; private set; }
        private readonly TrendDir trendDir;

        #region Constructors
        public HighLowPoint()
        {
            trendDir = TrendDir.Unknown;
        }

        public HighLowPoint(HighLowPoint hlp)
        {
            Price = hlp.Price;
            BarIndex = hlp.BarIndex;
            trendDir = TrendDir.Unknown;
        }

        public HighLowPoint(DowPivot dowPivot, double price, int barIndex, int pointIndex, TrendDir trendDir)
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
                        Miscellaneous.ConvertBarIndexToBarsAgo(dowPivot, BarIndex), 
                        Price, Brushes.Red).OutlineBrush = Brushes.Transparent;

                    Draw.Text(dowPivot, trendDir + " Text " + PointIndex.ToString(), false, PointIndex.ToString(),
                        Miscellaneous.ConvertBarIndexToBarsAgo(dowPivot, BarIndex),
                        Price, -15, Brushes.White,
                        new Gui.Tools.SimpleFont("Arial", 11),
                        System.Windows.TextAlignment.Center,
                        Brushes.Transparent, Brushes.Transparent, 100);
                    break;
                case TrendDir.Up:
                    Draw.Dot(dowPivot, (trendDir + " Dot " + PointIndex.ToString()), false,
                        Miscellaneous.ConvertBarIndexToBarsAgo(dowPivot, BarIndex), 
                        Price, Brushes.Green).OutlineBrush = Brushes.Transparent;

                    Draw.Text(dowPivot, trendDir + " Text " + PointIndex.ToString(), false, PointIndex.ToString(),
                        Miscellaneous.ConvertBarIndexToBarsAgo(dowPivot, BarIndex), 
                        Price, 15, Brushes.White,
                        new Gui.Tools.SimpleFont("Arial", 11),
                        System.Windows.TextAlignment.Center,
                        Brushes.Transparent, Brushes.Transparent, 100);
                    break;
            }

        }
    }
}