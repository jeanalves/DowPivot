using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators;
using System.Collections.Generic;

namespace NinjaTrader.Custom.Indicators.DowPivotBase
{
    public class SwingForwardCalculationNew : ZigZag
    {
        #region Fields
        private readonly int strength;
        private List<HighLowAndIndex> initLowCache = new List<HighLowAndIndex>();
        private List<HighLowAndIndex> initHighCache = new List<HighLowAndIndex>();
        private double lastLow;
        private double lastHigh;
        private TrendDir lastTrend;
        private CalculationEstate calculationEstate = CalculationEstate.FirstValue;
        private const int barsAgoConstant = 1;
        #endregion

        public SwingForwardCalculationNew(DowPivot dowPivot) : base(dowPivot)
        {
            strength = (int)dowPivot.Strength;
            lastTrend = TrendDir.Unknown;
        }

        public override void Calculate(DowPivot dowPivot)
        {
            ISeries<double> lows;
            ISeries<double> highs;

            if (dowPivot.UseHighLow)
            {
                lows = dowPivot.Low;
                highs = dowPivot.High;
            }
            else
            {
                lows = dowPivot.Close;
                highs = dowPivot.Close;
            }

            #region Calcula primeiro ponto
            // Este laço faz o calculo das primeiras barras do gráfico
            if (dowPivot.IsFirstTickOfBar && GetHigh(0) == null && GetLow(0) == null)
            {
                initLowCache.Add(new HighLowAndIndex(lows[barsAgoConstant], dowPivot.CurrentBar - barsAgoConstant));
                initHighCache.Add(new HighLowAndIndex(highs[barsAgoConstant], dowPivot.CurrentBar - barsAgoConstant));

                HighLowAndIndex lowCandidate = initLowCache[0];
                HighLowAndIndex highCandidate = initHighCache[0];

                if (initLowCache.Count - 1 >= strength)
                {
                    for (int i = 0; i < strength; i++)
                        if (initLowCache[i].Price < lowCandidate.Price)
                            lowCandidate = initLowCache[i];
                }

                if (initHighCache.Count - 1 >= strength)
                {
                    for (int i = 0; i < strength; i++)
                        if (initHighCache[i].Price > highCandidate.Price)
                            highCandidate = initHighCache[i];
                }

                if (initLowCache.Count - 1 >= strength && initHighCache.Count - 1 >= strength)
                {
                    if (lowCandidate.Index < highCandidate.Index)
                    {
                        AddLow(dowPivot, lowCandidate.Price, lowCandidate.Index);
                        lastTrend = TrendDir.Down;
                        calculationEstate = CalculationEstate.HistoricalRealTime;
                    }
                    else if (lowCandidate.Index > highCandidate.Index)
                    {
                        AddHigh(dowPivot, highCandidate.Price, highCandidate.Index);
                        lastTrend = TrendDir.Up;
                        calculationEstate = CalculationEstate.HistoricalRealTime;
                    }
                    else
                    {
                        Miscellaneous.PrintError(dowPivot, "In the initial calculation of the 'Low' and 'High', they have the same index," +
                            " Low index: " + lowCandidate.Index + "    High index: " + highCandidate.Index);
                    }
                }

            }
            #endregion

            #region Calcula dados históricos e de tempo real
            else if (dowPivot.IsFirstTickOfBar && calculationEstate == CalculationEstate.HistoricalRealTime)
            {
                bool isSwingLow = lows[barsAgoConstant] < lows[1 + barsAgoConstant];
                bool isSwingHigh = highs[barsAgoConstant] > highs[1 + barsAgoConstant];

                if (isSwingLow)
                {
                    for (int i = 1; i < strength + 1; i++)
                    {
                        if (lows[barsAgoConstant] > lows[i + barsAgoConstant])
                        {
                            isSwingLow = false;
                            break;
                        }
                    }
                }

                if (isSwingHigh)
                {
                    for (int i = 1; i < strength + 1; i++)
                    {
                        if (highs[barsAgoConstant] < highs[i + barsAgoConstant])
                        {
                            isSwingHigh = false;
                            break;
                        }
                    }
                }

                #region CRUD
                // Add low
                if (isSwingLow && lastTrend != TrendDir.Down)
                {
                    AddLow(dowPivot, lows[barsAgoConstant], dowPivot.CurrentBar - barsAgoConstant);
                    lastLow = lows[barsAgoConstant];
                    lastTrend = TrendDir.Down;
                }
                // Add high
                else if (isSwingHigh && lastTrend != TrendDir.Up)
                {
                    AddHigh(dowPivot, highs[barsAgoConstant], dowPivot.CurrentBar - barsAgoConstant);
                    lastHigh = highs[barsAgoConstant];
                    lastTrend = TrendDir.Up;
                }
                // Update Low
                else if (isSwingLow && lastTrend == TrendDir.Down && lows[barsAgoConstant] < lastLow)
                {
                    UpdateLow(dowPivot, lows[barsAgoConstant], dowPivot.CurrentBar - barsAgoConstant);
                    lastLow = lows[barsAgoConstant];
                }
                // Update High
                else if (isSwingHigh && lastTrend == TrendDir.Up && highs[barsAgoConstant] > lastHigh)
                {
                    UpdateHigh(dowPivot, highs[barsAgoConstant], dowPivot.CurrentBar - barsAgoConstant);
                    lastHigh = highs[barsAgoConstant];
                }
                #endregion
            }
            #endregion
        }

        private struct HighLowAndIndex
        {
            public double Price;
            public int Index;

            public HighLowAndIndex(double price, int index)
            {
                Price = price;
                Index = index;
            }
        }

        private enum CalculationEstate
        {
            FirstValue,
            HistoricalRealTime
        }
        public override TrendDir GetCurrentHighLowLeg()
        {
            return lastTrend;
        }
    }
}
