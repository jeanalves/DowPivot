using NinjaTrader.NinjaScript.Indicators;
using System.Collections;
using System.Collections.Generic;

namespace NinjaTrader.Custom.Indicators.DowPivotBase
{
    public class SwingForwardCalculationOld : ZigZag
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

        public SwingForwardCalculationOld(DowPivot dowPivot) : base(dowPivot)
        {
            strength = (int)dowPivot.Strength;

            lastLowCache = new ArrayList();
            lastHighCache = new ArrayList();

            lastTrend = TrendDir.Unknown;
        }

        public override void Calculate(DowPivot dowPivot)
        {
            #region Calcula primeiro ponto
            // Este laço faz o calculo das primeiras barras do gráfico
            if (dowPivot.IsFirstTickOfBar && GetHigh(0) == null && GetLow(0) == null)
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
            #endregion

            #region Calcula dados históricos e de tempo real
            // Enter only once per bar
            else if (dowPivot.IsFirstTickOfBar && (calculationEstate == CalculationEstate.SecondValue || calculationEstate == CalculationEstate.HistoricalRealTime))
            {
                bool isFalling = true;
                bool isRising = true;
                bool isOverLowStrength = false;
                bool isOverHighStrength = false;

                if (lastTrend == TrendDir.Up && calculationEstate == CalculationEstate.SecondValue)
                {
                    isOverLowStrength = (dowPivot.CurrentBar - GetHigh(0).BarIndex) >= strength;
                    calculationEstate = CalculationEstate.HistoricalRealTime;
                }
                else if (lastTrend == TrendDir.Down && calculationEstate == CalculationEstate.SecondValue)
                {
                    isOverHighStrength = (dowPivot.CurrentBar - GetLow(0).BarIndex) >= strength;
                    calculationEstate = CalculationEstate.HistoricalRealTime;
                }
                else
                {
                    isOverLowStrength = (dowPivot.CurrentBar - GetHigh(0).BarIndex) >= strength;
                    isOverHighStrength = (dowPivot.CurrentBar - GetLow(0).BarIndex) >= strength;
                }

                lastLowCache.Add(dowPivot.Low[barsAgoConstant]);
                if (lastLowCache.Count > strength)
                    lastLowCache.RemoveAt(0);
                double swingLowCandidateValue = (double)lastLowCache[lastLowCache.Count - 1];

                lastHighCache.Add(dowPivot.High[barsAgoConstant]);
                if (lastHighCache.Count > strength)
                    lastHighCache.RemoveAt(0);
                double swingHighCandidateValue = (double)lastHighCache[lastHighCache.Count - 1];

                // Low calculations
                if (lastLowCache.Count == strength)
                {
                    for (int i = 0; i < strength; i++)
                        if (swingLowCandidateValue > (double)lastLowCache[i])
                            isFalling = false;
                }
                // High calculations
                if (lastHighCache.Count == strength)
                {
                    for (int i = 0; i < strength; i++)
                        if (swingHighCandidateValue < (double)lastHighCache[i])
                            isRising = false;
                }

                // Add low
                if (isFalling && isOverLowStrength && lastTrend != TrendDir.Down)
                {
                    AddLow(dowPivot, dowPivot.Low[barsAgoConstant], dowPivot.CurrentBar - 1);
                    lastLow = dowPivot.Low[barsAgoConstant];
                    lastTrend = TrendDir.Down;
                }
                // Add high
                else if (isRising && isOverHighStrength && lastTrend != TrendDir.Up)
                {
                    AddHigh(dowPivot, dowPivot.High[barsAgoConstant], dowPivot.CurrentBar - 1);
                    lastHigh = dowPivot.High[barsAgoConstant];
                    lastTrend = TrendDir.Up;
                }
                // Update low
                else if (isFalling && lastTrend == TrendDir.Down && dowPivot.Low[barsAgoConstant] < lastLow)
                {
                    UpdateLow(dowPivot, dowPivot.Low[barsAgoConstant], dowPivot.CurrentBar - 1);
                    lastLow = dowPivot.Low[barsAgoConstant];
                }
                // Update high
                else if (isRising && lastTrend == TrendDir.Up && dowPivot.High[barsAgoConstant] > lastHigh)
                {
                    UpdateHigh(dowPivot, dowPivot.High[barsAgoConstant], dowPivot.CurrentBar - 1);
                    lastHigh = dowPivot.High[barsAgoConstant];
                }
            }
            #endregion

            #region Calcula todos os ticks em tempo real
            // Este "if" � executado apenas quando lastTrend � iniciada alterando de "Unknow"
            // para "Up" ou "Down" e em todos os ticks com exce��o do primeiro tick
            else if (!dowPivot.IsFirstTickOfBar && GetLow(0) != null && GetHigh(0) != null &&
                calculationEstate == CalculationEstate.HistoricalRealTime)
            {
                // Low logic 
                if (lastTrend == TrendDir.Down && dowPivot.Low[0] < GetLow(0).Price)
                {
                    UpdateLow(dowPivot, dowPivot.Low[0], dowPivot.CurrentBar);
                }

                // High logic 
                if (lastTrend == TrendDir.Up && dowPivot.High[0] > GetHigh(0).Price)
                {
                    UpdateHigh(dowPivot, dowPivot.High[0], dowPivot.CurrentBar);
                }
            }
            #endregion

        }

        // Substituir este Struct por System.Windows.Point
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
