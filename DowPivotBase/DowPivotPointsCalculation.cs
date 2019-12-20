using NinjaTrader.NinjaScript.Indicators;

namespace NinjaTrader.Custom.Indicators.DowPivotBase
{
    public class DowPivotPointsCalculation : DowPivotZigZag
    {
        #region Variables
        private double lastPrice;

        private bool isFirstLowValue;
        private bool isFirstHighValue;

        private bool isFalling;
        private bool isRising;

        private bool isOverLowPipDiff;
        private bool isOverHighPipDiff;
        #endregion

        public DowPivotPointsCalculation(DowPivot dowPivot) : base(dowPivot)
        {
            isFirstLowValue = true;
            isFirstHighValue = true;
        }

        public override void Calculate(DowPivot dowPivot)
        {
            //Calculation
            isFalling = GetCloseOrHighLow(dowPivot, TrendDir.Down, 0) < GetCloseOrHighLow(dowPivot, TrendDir.Down, 0 + 1);
            isRising = GetCloseOrHighLow(dowPivot, TrendDir.Up, 0) > GetCloseOrHighLow(dowPivot, TrendDir.Up, 0 + 1);

            isOverLowPipDiff = GetCloseOrHighLow(dowPivot, TrendDir.Down, 0) <= (GetHigh(0).Price - (dowPivot.Strength * (dowPivot.TickSize * 10)));
            isOverHighPipDiff = GetCloseOrHighLow(dowPivot, TrendDir.Up, 0) >= (GetLow(0).Price + (dowPivot.Strength * (dowPivot.TickSize * 10)));

            // Add low
            if (isFirstLowValue && isFalling && isOverLowPipDiff)
            {
                AddLow(dowPivot, GetCloseOrHighLow(dowPivot, TrendDir.Down, 0), dowPivot.CurrentBar);

                lastPrice = GetLow(0).Price;

                isFirstLowValue = false;
                isFirstHighValue = true;
            }
            //Add high
            else if (isFirstHighValue && isRising && isOverHighPipDiff)
            {
                AddHigh(dowPivot, GetCloseOrHighLow(dowPivot, TrendDir.Up, 0), dowPivot.CurrentBar);

                lastPrice = GetHigh(0).Price;

                isFirstLowValue = true;
                isFirstHighValue = false;
            }
            // Update low
            if (!isFirstLowValue && isFalling && isOverLowPipDiff && GetCloseOrHighLow(dowPivot, TrendDir.Down, 0) < lastPrice)
            {
                UpdateLow(dowPivot, GetCloseOrHighLow(dowPivot, TrendDir.Down, 0), dowPivot.CurrentBar);
                lastPrice = GetLow(0).Price;
            }
            // Update high
            else if (!isFirstHighValue && isRising && isOverHighPipDiff && GetCloseOrHighLow(dowPivot, TrendDir.Up, 0) > lastPrice)
            {
                UpdateHigh(dowPivot, GetCloseOrHighLow(dowPivot, TrendDir.Up, 0), dowPivot.CurrentBar);
                lastPrice = GetHigh(0).Price;
            }
        }

        #region Methods
        public override TrendDir GetCurrentHighLowLeg()
        {
            if (isFirstLowValue)
                return TrendDir.Up;
            else
                return TrendDir.Down;
        }
        private double GetCloseOrHighLow(DowPivot _dp, TrendDir trendDir, int _barsAgo)
        {
            if (trendDir == TrendDir.Down)
            {
                if (_dp.UseHighLow)
                    return _dp.Low[_barsAgo];
                else
                    return _dp.Close[_barsAgo];
            }
            else if (trendDir == TrendDir.Up)
            {
                if (_dp.UseHighLow)
                    return _dp.High[_barsAgo];
                else
                    return _dp.Close[_barsAgo];
            }
            return 0;
        }
        #endregion
    }
}
