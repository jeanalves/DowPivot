using NinjaTrader.NinjaScript.Indicators;
using System.Collections;
using System.Collections.Generic;

namespace NinjaTrader.Custom.Indicators.DowPivotBase
{
    public class SwingForwardCalculation : ZigZag
    {
        #region Fields
        private readonly int strength;
        private readonly ArrayList lastLowCache;
        private readonly ArrayList lastHighCache;
        private List<HighLowAndIndex> initLowCache = new List<HighLowAndIndex>();
        private List<HighLowAndIndex> initHighCache = new List<HighLowAndIndex>();
        private double lastLow;
        private double lastHigh;
        private TrendDir lastTrend;
        private CalculationEstate calculationEstate = CalculationEstate.FirstValue;
        private const int barsAgoConstant = 1;
        #endregion

        public SwingForwardCalculation(DowPivot dowPivot) : base(dowPivot)
        {
            strength = (int)dowPivot.Strength;

            lastLowCache = new ArrayList();
            lastHighCache = new ArrayList();

            lastTrend = TrendDir.Unknown;
        }

        public override void Calculate(DowPivot dowPivot)
        {
            // Este laço faz o calculo das primeiras barras do gráfico
            if(dowPivot.IsFirstTickOfBar && GetHigh(0) == null && GetLow(0) == null)
            {
                initLowCache.Add(new HighLowAndIndex(dowPivot.Low[barsAgoConstant], dowPivot.CurrentBar - barsAgoConstant));
                initHighCache.Add(new HighLowAndIndex(dowPivot.High[barsAgoConstant], dowPivot.CurrentBar - barsAgoConstant));

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
                        calculationEstate = CalculationEstate.SecondValue;
                    }
                    else if (lowCandidate.Index > highCandidate.Index)
                    {
                        AddHigh(dowPivot, highCandidate.Price, highCandidate.Index);
                        lastTrend = TrendDir.Up;
                        calculationEstate = CalculationEstate.SecondValue;
                    }
                    else
                    {
                        Miscellaneous.PrintError(dowPivot, "In the initial calculation of the 'Low' and 'High', they have the same index," +
                            " Low index: " + lowCandidate.Index + "    High index: " + highCandidate.Index);
                    }
                }
                
            }
            else if(dowPivot.IsFirstTickOfBar && calculationEstate == CalculationEstate.SecondValue)
            {
                bool isSwingLow = dowPivot.Low[0] < dowPivot.Low[1];
                bool isSwingHigh = dowPivot.High[0] > dowPivot.High[1];

                if(isSwingLow)
                {
                    for(int i = 1; i < strength + 1; i++)
                    {
                        if(dowPivot.Low[0] >= dowPivot.Low[i])
                        {
                            isSwingLow = false;
                            break;
                        }
                    }
                }

                if(isSwingHigh)
                {
                    for(int i = 1; i < strength+1; i++)
                    {
                        if(dowPivot.High[0] <= dowPivot.High[i])
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
                    AddLow(dowPivot, dowPivot.Low[0], dowPivot.CurrentBar);
                    lastLow = dowPivot.Low[0];
                    lastTrend = TrendDir.Down;
                }
                // Add high
                else if(isSwingHigh && lastTrend != TrendDir.Up)
                {
                    AddHigh(dowPivot, dowPivot.High[0], dowPivot.CurrentBar);
                    lastHigh = dowPivot.High[0];
                    lastTrend = TrendDir.Up;
                }
                // Update Low
                else if(isSwingLow && lastTrend == TrendDir.Down && dowPivot.Low[0] < lastLow)
                {
                    UpdateLow(dowPivot, dowPivot.Low[0], dowPivot.CurrentBar);
                    lastLow = dowPivot.Low[0];
                }
                // Update High
                else if(isSwingHigh && lastTrend == TrendDir.Up && dowPivot.High[0] > lastHigh)
                {
                    UpdateHigh(dowPivot, dowPivot.High[0], dowPivot.CurrentBar);
                    lastHigh = dowPivot.High[0];
                }
                #endregion
            }
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
            SecondValue,
            HistoricalRealTime
        }
        public override TrendDir GetCurrentHighLowLeg()
        {
            return lastTrend;
        }
    }
}
