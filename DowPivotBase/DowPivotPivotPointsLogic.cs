using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
using System.Windows.Media;

namespace NinjaTrader.Custom.Indicators.DowPivotBase
{
    public class DowPivotPivotPointsLogic
    {
        #region Variables
        private readonly DowPivotPivotPoint low = new DowPivotPivotPoint();
        private readonly DowPivotPivotPoint high = new DowPivotPivotPoint();

        private DowPivotPivotPoint currentPP;

        private bool isFalling;
        private bool isRising;

        private bool downFilter = true;
        private bool upFilter = true;

        private string line1;
        private string line2;
        private string lastHighLegTagLine3;
        private string lastLowLegTagLine3;

        private bool isDownLine3LegEnd;
        private bool isUpLine3LegEnd;

        private bool isDrawingPivots;

        private TrendDir lastTrend = TrendDir.Unknown;
        #endregion

        public DowPivotPivotPointsLogic(DowPivot dowPivot)
        {
            SetStopLossPrice(dowPivot, dowPivot.Input[0]);                // Initiate
            SetProfitTargetPrice(dowPivot, currentPP, TrendDir.Unknown);  // Initiate
            SetPlotBuyOrSell(dowPivot, TrendDir.Unknown);                 // Set signal to 0 before a entry
        }

        public void Calculate(DowPivot dowPivot, DowPivotZigZag dowPivotZigZag)
        {
            if (dowPivotZigZag.GetLow(1).Price == 0 ||
                dowPivotZigZag.GetLow(0).Price == 0 ||
                dowPivotZigZag.GetHigh(1).Price == 0 ||
                dowPivotZigZag.GetHigh(0).Price == 0)
                return;

            currentPP = new DowPivotPivotPoint(dowPivotZigZag.GetLow(1), dowPivotZigZag.GetLow(0), dowPivotZigZag.GetHigh(1), dowPivotZigZag.GetHigh(0));

            isFalling = currentPP.SecondLow.Price < currentPP.FirstLow.Price &&
                            currentPP.SecondHigh.Price < currentPP.FirstHigh.Price;

            isRising = currentPP.SecondLow.Price > currentPP.FirstLow.Price &&
                            currentPP.SecondHigh.Price > currentPP.FirstHigh.Price;

            downFilter = !IsOverMaxPercentPivotRetracement(dowPivot, TrendDir.Down, currentPP) &&
                            !IsOverMinPercentPivotRetracement(dowPivot, TrendDir.Down, currentPP);

            upFilter = !IsOverMaxPercentPivotRetracement(dowPivot, TrendDir.Up, currentPP) &&
                            !IsOverMinPercentPivotRetracement(dowPivot, TrendDir.Up, currentPP);

            // Add low pivot
            if (isFalling && downFilter && dowPivotZigZag.GetCurrentHighLowLeg() == TrendDir.Down && lastTrend != TrendDir.Down)
            {
                low.FirstLow = new DowPivotHighLowPoint(currentPP.FirstLow);
                low.SecondLow = new DowPivotHighLowPoint(currentPP.SecondLow);
                low.FirstHigh = new DowPivotHighLowPoint(currentPP.FirstHigh);
                low.SecondHigh = new DowPivotHighLowPoint(currentPP.SecondHigh);

                PrintPivots(dowPivot, Situation.AddLow);

                lastTrend = TrendDir.Down;

                isDownLine3LegEnd = false;

                SetPlotBuyOrSell(dowPivot, TrendDir.Down);
                SetStopLossPrice(dowPivot, low.SecondHigh.Price);
                SetProfitTargetPrice(dowPivot, currentPP, TrendDir.Down);
            }
            // Add high pivot
            else if (isRising && upFilter && dowPivotZigZag.GetCurrentHighLowLeg() == TrendDir.Up && lastTrend != TrendDir.Up)
            {
                high.FirstLow = new DowPivotHighLowPoint(currentPP.FirstLow);
                high.SecondLow = new DowPivotHighLowPoint(currentPP.SecondLow);
                high.FirstHigh = new DowPivotHighLowPoint(currentPP.FirstHigh);
                high.SecondHigh = new DowPivotHighLowPoint(currentPP.SecondHigh);

                PrintPivots(dowPivot, Situation.AddHigh);

                lastTrend = TrendDir.Up;

                isUpLine3LegEnd = false;

                SetPlotBuyOrSell(dowPivot, TrendDir.Up);
                SetStopLossPrice(dowPivot, high.SecondLow.Price);
                SetProfitTargetPrice(dowPivot, currentPP, TrendDir.Up);
            }
            // Update low pivot
            else if (isFalling && currentPP.SecondLow.Price < low.SecondLow.Price && !isDownLine3LegEnd && lastTrend == TrendDir.Down)
            {
                low.SecondLow = new DowPivotHighLowPoint(currentPP.SecondLow);
                PrintPivots(dowPivot, Situation.UpdateLow);
            }
            // Update high pivot
            else if (isRising && currentPP.SecondHigh.Price > high.SecondHigh.Price && !isUpLine3LegEnd && lastTrend == TrendDir.Up)
            {
                high.SecondHigh = new DowPivotHighLowPoint(currentPP.SecondHigh);
                PrintPivots(dowPivot, Situation.UpdateHigh);
            }

            if (!isDrawingPivots)
            {
                //lastTrend = TrendDir.Unknown;
            }

            // Help to define the pivot end draw
            if (dowPivotZigZag.GetCurrentHighLowLeg() == TrendDir.Up)
                isDownLine3LegEnd = true;
            else if (dowPivotZigZag.GetCurrentHighLowLeg() == TrendDir.Down)
                isUpLine3LegEnd = true;

            // Help to define if is drawing any pivot
            if (lastTrend == TrendDir.Down && isDownLine3LegEnd)
            {
                isDrawingPivots = false;
            }
            else if (lastTrend == TrendDir.Up && isUpLine3LegEnd)
            {
                isDrawingPivots = false;
            }
            else
            {
                isDrawingPivots = true;
            }
        }

        #region Methods
        private void PrintPivots(DowPivot dowPivot, Situation state)
        {
            line1 = "Line 1 " + dowPivot.CurrentBar;
            line2 = "Line 2 " + dowPivot.CurrentBar;

            switch (state)
            {
                case Situation.AddHigh:
                    lastHighLegTagLine3 = "Line 3 " + dowPivot.CurrentBar;
                    Draw.Line(dowPivot, line1, false, DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, high.FirstLow.BarIndex), high.FirstLow.Price,
                        DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, high.FirstHigh.BarIndex), high.FirstHigh.Price, Brushes.Green, DashStyleHelper.Solid, dowPivot.DrawProp.ZigZagWidth);
                    Draw.Line(dowPivot, line2, false, DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, high.FirstHigh.BarIndex), high.FirstHigh.Price,
                        DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, high.SecondLow.BarIndex), high.SecondLow.Price, Brushes.Green, DashStyleHelper.Solid, dowPivot.DrawProp.ZigZagWidth);
                    Draw.Line(dowPivot, lastHighLegTagLine3, false, DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, high.SecondLow.BarIndex), high.SecondLow.Price,
                        DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, high.SecondHigh.BarIndex), high.SecondHigh.Price, Brushes.Green, DashStyleHelper.Solid, dowPivot.DrawProp.ZigZagWidth);
                    break;
                case Situation.AddLow:
                    lastLowLegTagLine3 = "Line 3 " + dowPivot.CurrentBar;
                    Draw.Line(dowPivot, line1, false, DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, low.FirstHigh.BarIndex), low.FirstHigh.Price,
                        DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, low.FirstLow.BarIndex), low.FirstLow.Price, Brushes.Red, DashStyleHelper.Solid, dowPivot.DrawProp.ZigZagWidth);
                    Draw.Line(dowPivot, line2, false, DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, low.FirstLow.BarIndex), low.FirstLow.Price,
                        DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, low.SecondHigh.BarIndex), low.SecondHigh.Price, Brushes.Red, DashStyleHelper.Solid, dowPivot.DrawProp.ZigZagWidth);
                    Draw.Line(dowPivot, lastLowLegTagLine3, false, DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, low.SecondHigh.BarIndex), low.SecondHigh.Price,
                        DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, low.SecondLow.BarIndex), low.SecondLow.Price, Brushes.Red, DashStyleHelper.Solid, dowPivot.DrawProp.ZigZagWidth);
                    break;
                case Situation.UpdateHigh:
                    Draw.Line(dowPivot, lastHighLegTagLine3, false, DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, high.SecondLow.BarIndex), high.SecondLow.Price,
                        DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, high.SecondHigh.BarIndex), high.SecondHigh.Price, Brushes.Green, DashStyleHelper.Solid, dowPivot.DrawProp.ZigZagWidth);
                    break;
                case Situation.UpdateLow:
                    Draw.Line(dowPivot, lastLowLegTagLine3, false, DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, low.SecondHigh.BarIndex), low.SecondHigh.Price,
                        DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, low.SecondLow.BarIndex), low.SecondLow.Price, Brushes.Red, DashStyleHelper.Solid, dowPivot.DrawProp.ZigZagWidth);
                    break;
            }
        }
        private bool IsOverMaxPercentPivotRetracement(DowPivot dowPivot, TrendDir trendDir, DowPivotPivotPoint pp)
        {
            switch (trendDir)
            {
                case TrendDir.Down:
                    double nDown = pp.FirstHigh.Price - pp.FirstLow.Price < 0 ?
                                    (pp.FirstHigh.Price - pp.FirstLow.Price) * -1 :
                                    pp.FirstHigh.Price - pp.FirstLow.Price;
                    double downLimit = (dowPivot.FiboPivot.MaxPercentOfPivotRetraction / 100) * nDown;

                    downLimit += pp.FirstLow.Price;

                    if (pp.SecondHigh.Price > downLimit)
                        return true;

                    break;
                case TrendDir.Up:
                    double nUp = pp.FirstLow.Price - pp.FirstHigh.Price < 0 ?
                                    (pp.FirstLow.Price - pp.FirstHigh.Price) * -1 :
                                    pp.FirstLow.Price - pp.FirstHigh.Price;
                    double upLimit = (dowPivot.FiboPivot.MaxPercentOfPivotRetraction / 100) * nUp;

                    upLimit -= pp.FirstHigh.Price;
                    upLimit *= -1; // Inverte valor negativo para positivo

                    if (pp.SecondLow.Price < upLimit)
                        return true;

                    break;
            }
            return false;
        }
        private bool IsOverMinPercentPivotRetracement(DowPivot dowPivot, TrendDir trendDir, DowPivotPivotPoint pp)
        {
            switch (trendDir)
            {
                case TrendDir.Down:
                    double nDown = pp.FirstHigh.Price - pp.FirstLow.Price < 0 ?
                                    (pp.FirstHigh.Price - pp.FirstLow.Price) * -1 :
                                    pp.FirstHigh.Price - pp.FirstLow.Price;
                    double downLimit = (dowPivot.FiboPivot.MinPercentOfPivotRetraction / 100) * nDown;

                    downLimit += pp.FirstLow.Price;

                    if (pp.SecondHigh.Price < downLimit)
                        return true;

                    break;
                case TrendDir.Up:
                    double nUp = pp.FirstLow.Price - pp.FirstHigh.Price < 0 ?
                                    (pp.FirstLow.Price - pp.FirstHigh.Price) * -1 :
                                    pp.FirstLow.Price - pp.FirstHigh.Price;

                    double upLimite = (dowPivot.FiboPivot.MinPercentOfPivotRetraction / 100) * nUp;

                    upLimite -= pp.FirstHigh.Price;
                    upLimite *= -1; // Inverte valor negativo para positivo

                    if (pp.SecondLow.Price > upLimite)
                        return true;

                    break;
            }
            return false;
        }
        private void SetPlotBuyOrSell(DowPivot dowPivot, TrendDir trendDir)
        {
            switch (trendDir)
            {
                case TrendDir.Down:
                    dowPivot.EntrySignal[0] = -1;
                    break;
                case TrendDir.Up:
                    dowPivot.EntrySignal[0] = 1;
                    break;
                case TrendDir.Unknown:
                    dowPivot.EntrySignal[0] = 0;
                    break;
            }
        }
        private void SetStopLossPrice(DowPivot dowPivot, double price)
        {
            dowPivot.StopLossPriceSignal[0] = price;
        }
        private void SetProfitTargetPrice(DowPivot dowPivot, DowPivotPivotPoint pp, TrendDir trendDir)
        {
            if (dowPivot.ShowTargetAndStop)
            {
                // Set where will be drawn the finish target line
                const int endLine = -4;
                switch (trendDir)
                {
                    case TrendDir.Down:
                        double nDown = pp.SecondHigh.Price - pp.FirstLow.Price < 0 ?
                                        (pp.SecondHigh.Price - pp.FirstLow.Price) * -1 :
                                        pp.SecondHigh.Price - pp.FirstLow.Price;
                        double downTarget = (dowPivot.PercentProfitTargetFibo / 100) * nDown;

                        downTarget -= pp.SecondHigh.Price;
                        downTarget *= -1; // Inverte valor negativo para positivo

                        Draw.Line(dowPivot, dowPivot.CurrentBar.ToString(), false, DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, pp.FirstLow.BarIndex),
                            downTarget, endLine, downTarget, Brushes.Red, DashStyleHelper.Solid, 2);

                        dowPivot.ProfitTargetPriceSignal[0] = downTarget;
                        break;
                    case TrendDir.Up:
                        double nUp = pp.SecondLow.Price - pp.FirstHigh.Price < 0 ?
                                        (pp.SecondLow.Price - pp.FirstHigh.Price) * -1 :
                                        pp.SecondLow.Price - pp.FirstHigh.Price;
                        double upTarget = (dowPivot.PercentProfitTargetFibo / 100) * nUp;

                        upTarget += pp.SecondLow.Price;

                        Draw.Line(dowPivot, dowPivot.CurrentBar.ToString(), false, DowPivotMiscellaneous.ConvertBarIndexToBarsAgo(dowPivot, pp.FirstHigh.BarIndex),
                            upTarget, endLine, upTarget, Brushes.Green, DashStyleHelper.Solid, 2);

                        dowPivot.ProfitTargetPriceSignal[0] = upTarget;
                        break;
                    case TrendDir.Unknown:
                        dowPivot.ProfitTargetPriceSignal[0] = dowPivot.Input[0];
                        break;
                }
            }
        }
        #endregion
    }
}
