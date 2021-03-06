﻿using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
using System.Collections.Generic;

namespace NinjaTrader.Custom.Indicators.DowPivotBase
{
    public abstract class ZigZag : IZigZagBasicFunctions
    {
        private readonly List<HighLowPoint> lows;
        private readonly List<HighLowPoint> highs;
        private readonly DowPivotOld dowPivot;
        private string lowTagName;
        private string highTagName;
        public int PointCount { get; private set; }

        protected ZigZag(DowPivotOld dowPivot)
        {
            lows = new List<HighLowPoint>();
            highs = new List<HighLowPoint>();

            this.dowPivot = dowPivot;
            PointCount = 0;
        }

        #region GETs
        public HighLowPoint GetLow(int pointsAgo)
        {
            if (lows.Count == 0)
                return null;
            else if (lows.Count <= pointsAgo)
                return null;

            return lows[lows.Count - 1 - pointsAgo];
        }
        public HighLowPoint GetHigh(int pointsAgo)
        {
            if (highs.Count == 0)
                return null;
            else if (highs.Count <= pointsAgo)
                return null;

            return highs[highs.Count - 1 - pointsAgo];
        }
        #endregion

        #region SETs
        protected void AddLow(DowPivotOld dowPivot, double price, int barIndex)
        {
            lows.Add(new HighLowPoint(dowPivot, price, barIndex, PointCount, TrendDir.Down));
            PrintZigZagLines(dowPivot, Situation.AddLow);
            PointCount++;
        }
        protected void AddHigh(DowPivotOld dowPivot, double price, int barIndex)
        {
            highs.Add(new HighLowPoint(dowPivot, price, barIndex, PointCount, TrendDir.Up));
            PrintZigZagLines(dowPivot, Situation.AddHigh);
            PointCount++;
        }
        protected void UpdateLow(DowPivotOld dowPivot, double price, int barIndex)
        {
            lows[lows.Count - 1].Update(dowPivot, price, barIndex);
            PrintZigZagLines(dowPivot, Situation.UpdateLow);
        }
        protected void UpdateHigh(DowPivotOld dowPivot, double price, int barIndex)
        {
            highs[highs.Count - 1].Update(dowPivot, price, barIndex);
            PrintZigZagLines(dowPivot, Situation.UpdateHigh);
        }
        #endregion

        private void PrintZigZagLines(DowPivotOld dowPivot, Situation situation)
        {
            if (lows.Count != 0 && highs.Count != 0 && dowPivot.DrawProp.ShowZigZag)
            {
                switch (situation)
                {
                    case Situation.AddLow:
                        lowTagName = "Low line" + dowPivot.CurrentBar;
                        Draw.Line(dowPivot, lowTagName, false,
                            Miscellaneous.ConvertBarIndexToBarsAgo(dowPivot, GetHigh(0).BarIndex), GetHigh(0).Price,
                            Miscellaneous.ConvertBarIndexToBarsAgo(dowPivot, GetLow(0).BarIndex), GetLow(0).Price,
                            dowPivot.DrawProp.ZigZagColor, DashStyleHelper.Solid, dowPivot.DrawProp.ZigZagWidth);
                        break;
                    case Situation.AddHigh:
                        highTagName = "High line" + dowPivot.CurrentBar;
                        Draw.Line(dowPivot, highTagName, false,
                            Miscellaneous.ConvertBarIndexToBarsAgo(dowPivot, GetLow(0).BarIndex), GetLow(0).Price,
                            Miscellaneous.ConvertBarIndexToBarsAgo(dowPivot, GetHigh(0).BarIndex), GetHigh(0).Price,
                            dowPivot.DrawProp.ZigZagColor, DashStyleHelper.Solid, dowPivot.DrawProp.ZigZagWidth);
                        break;
                    case Situation.UpdateLow:
                        Draw.Line(dowPivot, lowTagName, false,
                            Miscellaneous.ConvertBarIndexToBarsAgo(dowPivot, GetHigh(0).BarIndex), GetHigh(0).Price,
                            Miscellaneous.ConvertBarIndexToBarsAgo(dowPivot, GetLow(0).BarIndex), GetLow(0).Price,
                            dowPivot.DrawProp.ZigZagColor, DashStyleHelper.Solid, dowPivot.DrawProp.ZigZagWidth);
                        break;
                    case Situation.UpdateHigh:
                        Draw.Line(dowPivot, highTagName, false,
                            Miscellaneous.ConvertBarIndexToBarsAgo(dowPivot, GetLow(0).BarIndex), GetLow(0).Price,
                            Miscellaneous.ConvertBarIndexToBarsAgo(dowPivot, GetHigh(0).BarIndex), GetHigh(0).Price,
                            dowPivot.DrawProp.ZigZagColor, DashStyleHelper.Solid, dowPivot.DrawProp.ZigZagWidth);
                        break;
                }
            }
        }

        #region Interface methods
        /// <summary>
        /// Used to create the magic
        /// </summary>
        /// <param name="dowPivot"></param>
        public abstract void Calculate(DowPivotOld dowPivot);
        /// <summary>
        /// This method tells you which trend leg is in formation at the moment.
        /// </summary>
        /// <returns></returns>
        public abstract TrendDir GetCurrentHighLowLeg();
        #endregion
    }
    public interface IZigZagBasicFunctions
    {
        void Calculate(DowPivotOld dowPivot);
        TrendDir GetCurrentHighLowLeg();
    }
}
