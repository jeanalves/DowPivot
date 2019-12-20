using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using System.Collections;

namespace NinjaTrader.Custom.Indicators.DowPivotBase
{
    public class DowPivotSwingCalculation : DowPivotZigZag
    {
        #region Variables
        private readonly int strength;
        private readonly int constant;
        private readonly ArrayList lastLowCache;
        private readonly ArrayList lastHighCache;
        private bool isSwingLow;
        private bool isSwingHigh;
        private double swingLowCandidateValue;
        private double swingHighCandidateValue;
        private double lastLow;
        private double lastHigh;
        private TrendDir lastTrend;
        /// <summary>
        /// Constante usada para calculos feitos em "OnPriceChange" ou "OnEachTick"
        /// se n�o usada na atualiza��o dos valores podera causar certa discrepancia neles
        /// </summary>
        private const int barsAgoConstant = 1;
        #endregion

        public DowPivotSwingCalculation(DowPivot dowPivot) : base(dowPivot)
        {
            strength = (int)dowPivot.Strength;
            constant = (strength * 2) + 1;

            lastLowCache = new ArrayList();
            lastHighCache = new ArrayList();

            lastTrend = TrendDir.Unknown;
        }

        public override void Calculate(DowPivot dowPivot)
        {
            // Este "if" � executado apenas quando lastTrend � iniciada alterando de "Unknow"
            // para "Up" ou "Down" e em todos os ticks com exce��o do primeiro tick
            if (!dowPivot.IsFirstTickOfBar & lastTrend != TrendDir.Unknown)
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
                lastLowCache.Add(dowPivot.Low[0 + barsAgoConstant]);
                if (lastLowCache.Count > constant)
                    lastLowCache.RemoveAt(0);

                lastHighCache.Add(dowPivot.High[0 + barsAgoConstant]);
                if (lastHighCache.Count > constant)
                    lastHighCache.RemoveAt(0);

                // Low calculations
                if (lastLowCache.Count == constant)
                {
                    isSwingLow = true;
                    swingLowCandidateValue = (double)lastLowCache[strength];

                    for (int i = 0; i < dowPivot.Strength; i++)
                        if (((double)lastLowCache[i]).ApproxCompare(swingLowCandidateValue) <= 0)
                            isSwingLow = false;

                    for (int i = strength + 1; i < lastLowCache.Count; i++)
                        if (((double)lastLowCache[i]).ApproxCompare(swingLowCandidateValue) < 0)
                            isSwingLow = false;
                }

                // High calculations
                if (lastHighCache.Count == constant)
                {
                    isSwingHigh = true;
                    swingHighCandidateValue = (double)lastHighCache[strength];

                    for (int i = 0; i < strength; i++)
                        if (((double)lastHighCache[i]).ApproxCompare(swingHighCandidateValue) >= 0)
                            isSwingHigh = false;

                    for (int i = strength + 1; i < lastHighCache.Count; i++)
                        if (((double)lastHighCache[i]).ApproxCompare(swingHighCandidateValue) > 0)
                            isSwingHigh = false;
                }

                // Add low
                if (isSwingLow && lastTrend != TrendDir.Down)
                {
                    AddLow(dowPivot, swingLowCandidateValue, (dowPivot.CurrentBar - strength) - barsAgoConstant);
                    lastLow = swingLowCandidateValue;
                    lastTrend = TrendDir.Down;
                }
                // Add high
                if (isSwingHigh && lastTrend != TrendDir.Up)
                {
                    AddHigh(dowPivot, swingHighCandidateValue, (dowPivot.CurrentBar - strength) - barsAgoConstant);
                    lastHigh = swingHighCandidateValue;
                    lastTrend = TrendDir.Up;
                }
                // Update low
                if (isSwingLow && lastTrend == TrendDir.Down && swingLowCandidateValue < lastLow)
                {
                    UpdateLow(dowPivot, swingLowCandidateValue, (dowPivot.CurrentBar - strength) - barsAgoConstant);
                    lastLow = swingLowCandidateValue;
                }
                // Update high
                if (isSwingHigh && lastTrend == TrendDir.Up && swingHighCandidateValue > lastHigh)
                {
                    UpdateHigh(dowPivot, swingHighCandidateValue, (dowPivot.CurrentBar - strength) - barsAgoConstant);
                    lastHigh = swingHighCandidateValue;
                }
            }
        }

        public override TrendDir GetCurrentHighLowLeg()
        {
            return lastTrend;
        }
    }
}
