using NinjaTrader.NinjaScript.Indicators;
using System.Collections;

namespace NinjaTrader.Custom.Indicators.DowPivotBase
{
    public class SwingForwardCalculation : ZigZag
    {
        #region Variables
        private readonly int strength;
        private readonly ArrayList lastLowCache;
        private readonly ArrayList lastHighCache;
        private double lastLow;
        private double lastHigh;
        private TrendDir lastTrend;
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
            // Este "if" � executado apenas quando lastTrend � iniciada alterando de "Unknow"
            // para "Up" ou "Down" e em todos os ticks com exce��o do primeiro tick
            if (!dowPivot.IsFirstTickOfBar && lastTrend != TrendDir.Unknown)
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
            // Enter only once per bar
            else if (dowPivot.IsFirstTickOfBar)
            {
                bool isFalling = false;
                bool isRising = false;

                bool isOverLowStrength = (dowPivot.CurrentBar - GetHigh(0).BarIndex) >= strength;
                bool isOverHighStrength = (dowPivot.CurrentBar - GetLow(0).BarIndex) >= strength;

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
                        if (swingLowCandidateValue < (double)lastLowCache[i])
                            isFalling = true;
                }
                // High calculations
                if (lastHighCache.Count == strength)
                {
                    for (int i = 0; i < strength; i++)
                        if (swingHighCandidateValue > (double)lastHighCache[i])
                            isRising = true;
                }

                // Add low
                if (isFalling && isOverLowStrength && lastTrend != TrendDir.Down)
                {
                    AddLow(dowPivot, dowPivot.Low[barsAgoConstant], dowPivot.CurrentBar - 1);
                    lastLow = dowPivot.Low[barsAgoConstant];
                    lastTrend = TrendDir.Down;
                }
                // Add high
                if (isRising && isOverHighStrength && lastTrend != TrendDir.Up)
                {
                    AddHigh(dowPivot, dowPivot.High[barsAgoConstant], dowPivot.CurrentBar - 1);
                    lastHigh = dowPivot.High[barsAgoConstant];
                    lastTrend = TrendDir.Up;
                }
                // Update low
                if (isFalling && isOverLowStrength && lastTrend == TrendDir.Down && dowPivot.Low[barsAgoConstant] < lastLow)
                {
                    UpdateLow(dowPivot, dowPivot.Low[barsAgoConstant], dowPivot.CurrentBar - 1);
                    lastLow = dowPivot.Low[barsAgoConstant];
                }
                // Update high
                if (isRising && isOverHighStrength && lastTrend == TrendDir.Up && dowPivot.High[barsAgoConstant] > lastHigh)
                {
                    UpdateHigh(dowPivot, dowPivot.High[barsAgoConstant], dowPivot.CurrentBar - 1);
                    lastHigh = dowPivot.High[barsAgoConstant];
                }
            }
        }

        public override TrendDir GetCurrentHighLowLeg()
        {
            return lastTrend;
        }
    }
}
