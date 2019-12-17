using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
using System.Collections.Generic;

namespace NinjaTrader.Custom.Indicators.DowPivotBase
{
    public abstract class DowPivotZigZag : IZigZagBasicFunctions
    {
        private readonly List<DowPivotHighLowPoint> lows;
        private readonly List<DowPivotHighLowPoint> highs;
        private string lowTagName;
        private string highTagName;

        protected DowPivotZigZag(DowPivot dowPivot)
        {
            lows = new List<DowPivotHighLowPoint>();
            highs = new List<DowPivotHighLowPoint>();

            //Inicia lista de HLPoints adcionando 4 novos objetos
            AddLow(dowPivot, dowPivot.Input[0], dowPivot.CurrentBar);
            AddLow(dowPivot, dowPivot.Input[0], dowPivot.CurrentBar);
            AddHigh(dowPivot, dowPivot.Input[0], dowPivot.CurrentBar);
            AddHigh(dowPivot, dowPivot.Input[0], dowPivot.CurrentBar);
        }

        #region GETs
        public DowPivotHighLowPoint GetLow(int pointsAgo)
        {
            return lows[lows.Count - 1 - pointsAgo];
        }
        public DowPivotHighLowPoint GetHigh(int pointsAgo)
        {
            return highs[highs.Count - 1 - pointsAgo];
        }
        #endregion

        #region SETs
        protected void AddLow(DowPivot dowPivot, double price, int barIndex)
        {
            lows.Add(new DowPivotHighLowPoint(dowPivot, price, barIndex, lows.Count, TrendDir.Down));
            PrintZigZagLines(dowPivot, Situation.AddLow);
        }
        protected void AddHigh(DowPivot dowPivot, double price, int barIndex)
        {
            highs.Add(new DowPivotHighLowPoint(dowPivot, price, barIndex, highs.Count, TrendDir.Up));
            PrintZigZagLines(dowPivot, Situation.AddHigh);
        }
        protected void UpdateLow(DowPivot dowPivot, double price, int barIndex)
        {
            lows[lows.Count - 1].Update(dowPivot, price, barIndex);
            PrintZigZagLines(dowPivot, Situation.UpdateLow);
        }
        protected void UpdateHigh(DowPivot dowPivot, double price, int barIndex)
        {
            highs[highs.Count - 1].Update(dowPivot, price, barIndex);
            PrintZigZagLines(dowPivot, Situation.UpdateHigh);
        }
        #endregion

        private void PrintZigZagLines(DowPivot dowPivot, Situation situation)
        {
            if (lows.Count > 2 && highs.Count > 2 && dowPivot.DrawProp.ShowZigZag)
            {
                switch (situation)
                {
                    case Situation.AddLow:
                        lowTagName = "Low line" + dowPivot.CurrentBar;
                        Draw.Line(dowPivot, lowTagName, false,
                            DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, GetHigh(0).BarIndex), GetHigh(0).Price,
                            DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, GetLow(0).BarIndex), GetLow(0).Price,
                            dowPivot.DrawProp.ZigZagColor, DashStyleHelper.Solid, dowPivot.DrawProp.ZigZagWidth);
                        break;
                    case Situation.AddHigh:
                        highTagName = "High line" + dowPivot.CurrentBar;
                        Draw.Line(dowPivot, highTagName, false,
                            DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, GetLow(0).BarIndex), GetLow(0).Price,
                            DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, GetHigh(0).BarIndex), GetHigh(0).Price,
                            dowPivot.DrawProp.ZigZagColor, DashStyleHelper.Solid, dowPivot.DrawProp.ZigZagWidth);
                        break;
                    case Situation.UpdateLow:
                        Draw.Line(dowPivot, lowTagName, false,
                            DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, GetHigh(0).BarIndex), GetHigh(0).Price,
                            DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, GetLow(0).BarIndex), GetLow(0).Price,
                            dowPivot.DrawProp.ZigZagColor, DashStyleHelper.Solid, dowPivot.DrawProp.ZigZagWidth);
                        break;
                    case Situation.UpdateHigh:
                        Draw.Line(dowPivot, highTagName, false,
                            DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, GetLow(0).BarIndex), GetLow(0).Price,
                            DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, GetHigh(0).BarIndex), GetHigh(0).Price,
                            dowPivot.DrawProp.ZigZagColor, DashStyleHelper.Solid, dowPivot.DrawProp.ZigZagWidth);
                        break;
                }
            }
        }

        #region Interface methods
        public abstract void Calculate(DowPivot dowPivot);
        /// <summary>
        /// This method tells you which trend leg is in formation at the moment.
        /// </summary>
        /// <returns></returns>
        public abstract TrendDir GetCurrentHighLowLeg();
        #endregion
    }
    public interface IZigZagBasicFunctions
    {
        void Calculate(DowPivot dowPivot);
        TrendDir GetCurrentHighLowLeg();
    }
}
