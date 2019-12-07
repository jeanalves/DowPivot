#region Using declarations
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.DrawingTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    public class DowPivot : Indicator
    {
        private SwingCalculation sc;
        private PointsCalculation pc;
        private PivotPointsLogic ppl;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Indicator here.";
                Name = "DowPivot";
                Calculate = Calculate.OnEachTick;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = false;
                DrawVerticalGridLines = false;
                PaintPriceMarkers = false;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;

                // Zig Zag parameters
                CalculationType = ZigZagCalculationType.Swing;
                Strength = 5;
                UseHighLow = true;
                DrawProp = new DrawProperties()
                {
                    ShowTopBottomPoints = true,
                    ShowZigZag = true,
                    ZigZagWidth = 3,
                    ZigZagColor = Brushes.White
                };

                // Pivot parameters
                ShowTargetAndStop = false;
                PercentProfitTargetFibo = 161.8;
                FiboPivot = new FiboPivotRetraction()
                {
                    MaxPercentOfPivotRetraction = 95,
                    MinPercentOfPivotRetraction = 38.2
                };

                AddPlot(Brushes.Transparent, "EntrySignal"); // -1 go short, 0 no signal and 1 go long
                AddPlot(Brushes.Transparent, "StopLossPriceSignal");
                AddPlot(Brushes.Transparent, "ProfitTargetPriceSignal");
            }
            else if (State == State.Configure)
            {
            }
            else if (State == State.DataLoaded)
            {
                sc = new SwingCalculation(this);
                pc = new PointsCalculation(this);
                ppl = new PivotPointsLogic(this);

                // Toda vez que a tecla F5 for pressionada automaticamente passara pelo metodo
                // ClearOutputWindow() e limpara a janela Output das saidas anteriores.
                ClearOutputWindow();
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 10)
                return;

            try
            {
                switch (CalculationType)
                {
                    case ZigZagCalculationType.Swing:
                        sc.Calculate(this);
                        ppl.Calculate(this, sc);
                        break;
                    case ZigZagCalculationType.Points:
                        pc.Calculate(this);
                        ppl.Calculate(this, pc);
                        break;
                }
            }
            catch (Exception e)
            {
                Print(e.ToString());
                Print("Current bar: " + CurrentBar);
            }
        }

        #region Properties
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> EntrySignal
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> StopLossPriceSignal
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ProfitTargetPriceSignal
        {
            get { return Values[2]; }
        }

        /************************************** Zig Zag parameters **************************************/
        [NinjaScriptProperty]
        [Display(Name = "Calculation type", Order = 0, GroupName = "1 Zig Zag parameters")]
        public ZigZagCalculationType CalculationType
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Strength", Order = 1, GroupName = "1 Zig Zag parameters")]
        public double Strength
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "UseHighLow", Order = 2, GroupName = "1 Zig Zag parameters")]
        public bool UseHighLow
        { get; set; }

        // Sub menu of draw objects from zigzag
        [NinjaScriptProperty]
        [Display(Name = "Draw properties", Order = 5, GroupName = "1 Zig Zag parameters")]
        public DrawProperties DrawProp
        { get; set; }
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class DrawProperties
        {
            public override string ToString()
            {
                return "";
            }

            // First draw propertie
            [XmlIgnore]
            [Display(Name = "Show Top Bottom Points", Order = 0, GroupName = "1 Zig Zag parameters")]
            public bool ShowTopBottomPoints
            { get; set; }
            // Serializable from first draw propertie
            [Browsable(false)]
            public string ShowTopBottomPointsSerializable
            {
                get { return ShowTopBottomPoints.ToString(); }
                set { ShowTopBottomPoints = Convert.ToBoolean(value); }
            }

            // Second draw propertie
            [XmlIgnore]
            [Display(Name = "Show zig zag", Order = 1, GroupName = "1 Zig Zag parameters")]
            public bool ShowZigZag
            { get; set; }
            // Serializable from second draw propertie
            [Browsable(false)]
            public string ShowZigZagSerializable
            {
                get { return ShowZigZag.ToString(); }
                set { ShowZigZag = Convert.ToBoolean(value); }
            }

            // Third draw propertie
            [XmlIgnore]
            [Display(Name = "Zig Zag width", Order = 2, GroupName = "1 Zig Zag parameters")]
            public int ZigZagWidth
            { get; set; }
            // Serializable from third draw propertie
            [Browsable(false)]
            public string ZigZagWidthSerializable
            {
                get { return ZigZagWidth.ToString(); }
                set { ZigZagWidth = Convert.ToInt32(value); }
            }

            // Fourth draw propertie
            [XmlIgnore]
            [Display(Name = "Zig zag color", Order = 3, GroupName = "1 Zig Zag parameters")]
            public Brush ZigZagColor
            { get; set; }
            // Serializable from fourth draw propertie
            [Browsable(false)]
            public string ZigZagColorSerializable
            {
                get { return Serialize.BrushToString(ZigZagColor); }
                set { ZigZagColor = Serialize.StringToBrush(value); }
            }
        }

        /**************************************** Pivot parameters ****************************************/

        [Display(Name = "Show target and stop", Order = 0, GroupName = "2 Pivot parameters")]
        public bool ShowTargetAndStop
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, 300)]
        [Display(Name = "Perc. profit target fibo", Order = 1, GroupName = "2 Pivot parameters")]
        public double PercentProfitTargetFibo
        { get; set; }

        // Sub menu of pivot retractions filters
        [NinjaScriptProperty]
        [Display(Name = "Fibo Pivot Retraction", Order = 3, GroupName = "2 Pivot parameters")]
        public FiboPivotRetraction FiboPivot
        { get; set; }
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class FiboPivotRetraction
        {
            public override string ToString()
            {
                return "Max " + MaxPercentOfPivotRetraction + "%, Min " + MinPercentOfPivotRetraction + "%";
            }

            // First double text box
            [XmlIgnore]
            [Range(0, 100)]
            [Display(Name = "Max percentage", Order = 0)]
            public double MaxPercentOfPivotRetraction
            { get; set; }
            //Serializable from first box
            [Browsable(false)]
            public string MaxPercentOfPivotRetractionSerializable
            {
                get { return MaxPercentOfPivotRetraction.ToString(); }
                set { MaxPercentOfPivotRetraction = Convert.ToDouble(value); }
            }

            //Second double text box
            [XmlIgnore]
            [Range(0, 100)]
            [Display(Name = "Min percentage", Order = 1)]
            public double MinPercentOfPivotRetraction
            { get; set; }
            //Serializable from second box
            [Browsable(false)]
            public string MinPercentOfPivotRetractionSerializable
            {
                get { return MinPercentOfPivotRetraction.ToString(); }
                set { MinPercentOfPivotRetraction = Convert.ToDouble(value); }
            }
        }
        #endregion
    }

    public class SwingCalculation : ZigZagDP
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

        public SwingCalculation(DowPivot dp) : base(dp)
        {
            strength = (int)dp.Strength;
            constant = (strength * 2) + 1;

            lastLowCache = new ArrayList();
            lastHighCache = new ArrayList();

            lastTrend = TrendDir.Unknown;
        }

        public override void Calculate(DowPivot dp)
        {
            // Este "if" � executado apenas quando lastTrend � iniciada alterando de "Unknow"
            // para "Up" ou "Down" e em todos os ticks com exce��o do primeiro tick
            if (!dp.IsFirstTickOfBar & lastTrend != TrendDir.Unknown)
            {
                ArrayList lastLowCacheTick = new ArrayList(lastLowCache);
                ArrayList lastHighCacheTick = new ArrayList(lastHighCache);

                lastLowCacheTick[lastLowCacheTick.Count - 1] = dp.Low[0];
                lastHighCacheTick[lastHighCacheTick.Count - 1] = dp.High[0];

                // Low calculations
                bool isSwingLowTick = true;
                double swingLowCandidateValueTick = (double)lastLowCacheTick[strength];

                for (int i = 0; i < dp.Strength; i++)
                    if (((double)lastLowCache[i]).ApproxCompare(swingLowCandidateValueTick) <= 0)
                        isSwingLowTick = false;

            }
            // Enter only once per bar
            else if (dp.IsFirstTickOfBar)
            {
                lastLowCache.Add(dp.Low[0 + barsAgoConstant]);
                if (lastLowCache.Count > constant)
                    lastLowCache.RemoveAt(0);

                lastHighCache.Add(dp.High[0 + barsAgoConstant]);
                if (lastHighCache.Count > constant)
                    lastHighCache.RemoveAt(0);

                // Low calculations
                if (lastLowCache.Count == constant)
                {
                    isSwingLow = true;
                    swingLowCandidateValue = (double)lastLowCache[strength];

                    for (int i = 0; i < dp.Strength; i++)
                        if (((double)lastLowCache[i]).ApproxCompare(swingLowCandidateValue) <= 0)
                            isSwingLow = false;

                    for (int i = Convert.ToInt32(dp.Strength) + 1; i < lastLowCache.Count; i++)
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
                    AddLow(dp, swingLowCandidateValue, (dp.CurrentBar - strength) - barsAgoConstant);
                    lastLow = swingLowCandidateValue;
                    lastTrend = TrendDir.Down;
                }
                // Add high
                if (isSwingHigh && lastTrend != TrendDir.Up)
                {
                    AddHigh(dp, swingHighCandidateValue, (dp.CurrentBar - strength) - barsAgoConstant);
                    lastHigh = swingHighCandidateValue;
                    lastTrend = TrendDir.Up;
                }
                // Update low
                if (isSwingLow && lastTrend == TrendDir.Down && swingLowCandidateValue < lastLow)
                {
                    UpdateLow(dp, swingLowCandidateValue, (dp.CurrentBar - strength) - barsAgoConstant);
                    lastLow = swingLowCandidateValue;
                }
                // Update high
                if (isSwingHigh && lastTrend == TrendDir.Up && swingHighCandidateValue > lastHigh)
                {
                    UpdateHigh(dp, swingHighCandidateValue, (dp.CurrentBar - strength) - barsAgoConstant);
                    lastHigh = swingHighCandidateValue;
                }
            }
        }

        public override TrendDir GetCurrentHighLowLeg()
        {
            return lastTrend;
        }
    }

    public class PointsCalculation : ZigZagDP
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

        public PointsCalculation(DowPivot dp) : base(dp)
        {
            isFirstLowValue = true;
            isFirstHighValue = true;
        }

        public override void Calculate(DowPivot dp)
        {
            //Calculation
            isFalling = GetCloseOrHighLow(dp, TrendDir.Down, 0) < GetCloseOrHighLow(dp, TrendDir.Down, 0 + 1);
            isRising = GetCloseOrHighLow(dp, TrendDir.Up, 0) > GetCloseOrHighLow(dp, TrendDir.Up, 0 + 1);

            isOverLowPipDiff = GetCloseOrHighLow(dp, TrendDir.Down, 0) <= (GetHigh(0).Price - (dp.Strength * (dp.TickSize * 10)));
            isOverHighPipDiff = GetCloseOrHighLow(dp, TrendDir.Up, 0) >= (GetLow(0).Price + (dp.Strength * (dp.TickSize * 10)));

            // Add low
            if (isFirstLowValue && isFalling && isOverLowPipDiff)
            {
                AddLow(dp, GetCloseOrHighLow(dp, TrendDir.Down, 0), dp.CurrentBar);

                lastPrice = GetLow(0).Price;

                isFirstLowValue = false;
                isFirstHighValue = true;
            }
            //Add high
            else if (isFirstHighValue && isRising && isOverHighPipDiff)
            {
                AddHigh(dp, GetCloseOrHighLow(dp, TrendDir.Up, 0), dp.CurrentBar);

                lastPrice = GetHigh(0).Price;

                isFirstLowValue = true;
                isFirstHighValue = false;
            }
            // Update low
            if (!isFirstLowValue && isFalling && isOverLowPipDiff && GetCloseOrHighLow(dp, TrendDir.Down, 0) < lastPrice)
            {
                UpdateLow(dp, GetCloseOrHighLow(dp, TrendDir.Down, 0), dp.CurrentBar);
                lastPrice = GetLow(0).Price;
            }
            // Update high
            else if (!isFirstHighValue && isRising && isOverHighPipDiff && GetCloseOrHighLow(dp, TrendDir.Up, 0) > lastPrice)
            {
                UpdateHigh(dp, GetCloseOrHighLow(dp, TrendDir.Up, 0), dp.CurrentBar);
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

    public class PivotPointsLogic
    {
        #region Variables
        private readonly PivotPoint low = new PivotPoint();
        private readonly PivotPoint high = new PivotPoint();

        private PivotPoint currentPP;

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

        public PivotPointsLogic(DowPivot dp)
        {
            SetStopLossPrice(dp, dp.Input[0]);                      // Initiate
            SetProfitTargetPrice(dp, currentPP, TrendDir.Unknown);  // Initiate
            SetPlotBuyOrSell(dp, TrendDir.Unknown);                 // Set signal to 0 before a entry
        }

        public void Calculate(DowPivot dp, ZigZagDP zigZagDP)
        {
            if (zigZagDP.GetLow(1).Price == 0 ||
                zigZagDP.GetLow(0).Price == 0 ||
                zigZagDP.GetHigh(1).Price == 0 ||
                zigZagDP.GetHigh(0).Price == 0)
                return;

            currentPP = new PivotPoint(zigZagDP.GetLow(1), zigZagDP.GetLow(0), zigZagDP.GetHigh(1), zigZagDP.GetHigh(0));

            isFalling = currentPP.low2.Price < currentPP.low1.Price &&
                            currentPP.high2.Price < currentPP.high1.Price;

            isRising = currentPP.low2.Price > currentPP.low1.Price &&
                            currentPP.high2.Price > currentPP.high1.Price;

            downFilter = !IsOverMaxPercentPivotRetracement(dp, TrendDir.Down, currentPP) &&
                            !IsOverMinPercentPivotRetracement(dp, TrendDir.Down, currentPP);

            upFilter = !IsOverMaxPercentPivotRetracement(dp, TrendDir.Up, currentPP) &&
                            !IsOverMinPercentPivotRetracement(dp, TrendDir.Up, currentPP);

            // Add low pivot
            if (isFalling && downFilter && zigZagDP.GetCurrentHighLowLeg() == TrendDir.Down && lastTrend != TrendDir.Down)
            {
                low.low1 = new HighLowPoint(currentPP.low1);
                low.low2 = new HighLowPoint(currentPP.low2);
                low.high1 = new HighLowPoint(currentPP.high1);
                low.high2 = new HighLowPoint(currentPP.high2);

                PrintPivots(dp, Situation.AddLow);

                lastTrend = TrendDir.Down;

                isDownLine3LegEnd = false;

                SetPlotBuyOrSell(dp, TrendDir.Down);
                SetStopLossPrice(dp, low.high2.Price);
                SetProfitTargetPrice(dp, currentPP, TrendDir.Down);
            }
            // Add high pivot
            else if (isRising && upFilter && zigZagDP.GetCurrentHighLowLeg() == TrendDir.Up && lastTrend != TrendDir.Up)
            {
                high.low1 = new HighLowPoint(currentPP.low1);
                high.low2 = new HighLowPoint(currentPP.low2);
                high.high1 = new HighLowPoint(currentPP.high1);
                high.high2 = new HighLowPoint(currentPP.high2);

                PrintPivots(dp, Situation.AddHigh);

                lastTrend = TrendDir.Up;

                isUpLine3LegEnd = false;

                SetPlotBuyOrSell(dp, TrendDir.Up);
                SetStopLossPrice(dp, high.low2.Price);
                SetProfitTargetPrice(dp, currentPP, TrendDir.Up);
            }
            // Update low pivot
            else if (isFalling && currentPP.low2.Price < low.low2.Price && !isDownLine3LegEnd && lastTrend == TrendDir.Down)
            {
                low.low2 = new HighLowPoint(currentPP.low2);
                PrintPivots(dp, Situation.UpdateLow);
            }
            // Update high pivot
            else if (isRising && currentPP.high2.Price > high.high2.Price && !isUpLine3LegEnd && lastTrend == TrendDir.Up)
            {
                high.high2 = new HighLowPoint(currentPP.high2);
                PrintPivots(dp, Situation.UpdateHigh);
            }

            if (!isDrawingPivots)
            {
                //lastTrend = TrendDir.Unknown;
            }

            // Help to define the pivot end draw
            if (zigZagDP.GetCurrentHighLowLeg() == TrendDir.Up)
                isDownLine3LegEnd = true;
            else if (zigZagDP.GetCurrentHighLowLeg() == TrendDir.Down)
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
        private void PrintPivots(DowPivot dp, Situation state)
        {
            line1 = "Line 1 " + dp.CurrentBar;
            line2 = "Line 2 " + dp.CurrentBar;

            switch (state)
            {
                case Situation.AddHigh:
                    lastHighLegTagLine3 = "Line 3 " + dp.CurrentBar;
                    Draw.Line(dp, line1, false, Miscellaneous.ConvertBarIndexToBarsAgo(dp, high.low1.BarIndex), high.low1.Price,
                        Miscellaneous.ConvertBarIndexToBarsAgo(dp, high.high1.BarIndex), high.high1.Price, Brushes.Green, DashStyleHelper.Solid, dp.DrawProp.ZigZagWidth);
                    Draw.Line(dp, line2, false, Miscellaneous.ConvertBarIndexToBarsAgo(dp, high.high1.BarIndex), high.high1.Price,
                        Miscellaneous.ConvertBarIndexToBarsAgo(dp, high.low2.BarIndex), high.low2.Price, Brushes.Green, DashStyleHelper.Solid, dp.DrawProp.ZigZagWidth);
                    Draw.Line(dp, lastHighLegTagLine3, false, Miscellaneous.ConvertBarIndexToBarsAgo(dp, high.low2.BarIndex), high.low2.Price,
                        Miscellaneous.ConvertBarIndexToBarsAgo(dp, high.high2.BarIndex), high.high2.Price, Brushes.Green, DashStyleHelper.Solid, dp.DrawProp.ZigZagWidth);
                    break;
                case Situation.AddLow:
                    lastLowLegTagLine3 = "Line 3 " + dp.CurrentBar;
                    Draw.Line(dp, line1, false, Miscellaneous.ConvertBarIndexToBarsAgo(dp, low.high1.BarIndex), low.high1.Price,
                        Miscellaneous.ConvertBarIndexToBarsAgo(dp, low.low1.BarIndex), low.low1.Price, Brushes.Red, DashStyleHelper.Solid, dp.DrawProp.ZigZagWidth);
                    Draw.Line(dp, line2, false, Miscellaneous.ConvertBarIndexToBarsAgo(dp, low.low1.BarIndex), low.low1.Price,
                        Miscellaneous.ConvertBarIndexToBarsAgo(dp, low.high2.BarIndex), low.high2.Price, Brushes.Red, DashStyleHelper.Solid, dp.DrawProp.ZigZagWidth);
                    Draw.Line(dp, lastLowLegTagLine3, false, Miscellaneous.ConvertBarIndexToBarsAgo(dp, low.high2.BarIndex), low.high2.Price,
                        Miscellaneous.ConvertBarIndexToBarsAgo(dp, low.low2.BarIndex), low.low2.Price, Brushes.Red, DashStyleHelper.Solid, dp.DrawProp.ZigZagWidth);
                    break;
                case Situation.UpdateHigh:
                    Draw.Line(dp, lastHighLegTagLine3, false, Miscellaneous.ConvertBarIndexToBarsAgo(dp, high.low2.BarIndex), high.low2.Price,
                        Miscellaneous.ConvertBarIndexToBarsAgo(dp, high.high2.BarIndex), high.high2.Price, Brushes.Green, DashStyleHelper.Solid, dp.DrawProp.ZigZagWidth);
                    break;
                case Situation.UpdateLow:
                    Draw.Line(dp, lastLowLegTagLine3, false, Miscellaneous.ConvertBarIndexToBarsAgo(dp, low.high2.BarIndex), low.high2.Price,
                        Miscellaneous.ConvertBarIndexToBarsAgo(dp, low.low2.BarIndex), low.low2.Price, Brushes.Red, DashStyleHelper.Solid, dp.DrawProp.ZigZagWidth);
                    break;
            }
        }
        private bool IsOverMaxPercentPivotRetracement(DowPivot dp, TrendDir trendDir, PivotPoint pp)
        {
            switch (trendDir)
            {
                case TrendDir.Down:
                    double nDown = pp.high1.Price - pp.low1.Price < 0 ?
                                    (pp.high1.Price - pp.low1.Price) * -1 :
                                    pp.high1.Price - pp.low1.Price;
                    double downLimit = (dp.FiboPivot.MaxPercentOfPivotRetraction / 100) * nDown;

                    downLimit += pp.low1.Price;

                    if (pp.high2.Price > downLimit)
                        return true;

                    break;
                case TrendDir.Up:
                    double nUp = pp.low1.Price - pp.high1.Price < 0 ?
                                    (pp.low1.Price - pp.high1.Price) * -1 :
                                    pp.low1.Price - pp.high1.Price;
                    double upLimit = (dp.FiboPivot.MaxPercentOfPivotRetraction / 100) * nUp;

                    upLimit -= pp.high1.Price;
                    upLimit *= -1; // Inverte valor negativo para positivo

                    if (pp.low2.Price < upLimit)
                        return true;

                    break;
            }
            return false;
        }
        private bool IsOverMinPercentPivotRetracement(DowPivot dp, TrendDir trendDir, PivotPoint pp)
        {
            switch (trendDir)
            {
                case TrendDir.Down:
                    double nDown = pp.high1.Price - pp.low1.Price < 0 ?
                                    (pp.high1.Price - pp.low1.Price) * -1 :
                                    pp.high1.Price - pp.low1.Price;
                    double downLimit = (dp.FiboPivot.MinPercentOfPivotRetraction / 100) * nDown;

                    downLimit += pp.low1.Price;

                    if (pp.high2.Price < downLimit)
                        return true;

                    break;
                case TrendDir.Up:
                    double nUp = pp.low1.Price - pp.high1.Price < 0 ?
                                    (pp.low1.Price - pp.high1.Price) * -1 :
                                    pp.low1.Price - pp.high1.Price;

                    double upLimite = (dp.FiboPivot.MinPercentOfPivotRetraction / 100) * nUp;

                    upLimite -= pp.high1.Price;
                    upLimite *= -1; // Inverte valor negativo para positivo

                    if (pp.low2.Price > upLimite)
                        return true;

                    break;
            }
            return false;
        }
        private void SetPlotBuyOrSell(DowPivot dp, TrendDir trendDir)
        {
            switch (trendDir)
            {
                case TrendDir.Down:
                    dp.EntrySignal[0] = -1;
                    break;
                case TrendDir.Up:
                    dp.EntrySignal[0] = 1;
                    break;
                case TrendDir.Unknown:
                    dp.EntrySignal[0] = 0;
                    break;
            }
        }
        private void SetStopLossPrice(DowPivot dp, double price)
        {
            dp.StopLossPriceSignal[0] = price;
        }
        private void SetProfitTargetPrice(DowPivot dp, PivotPoint pp, TrendDir trendDir)
        {
            if (dp.ShowTargetAndStop)
            {
                // Set where will be drawn the finish target line
                const int endLine = -4;
                switch (trendDir)
                {
                    case TrendDir.Down:
                        double nDown = pp.high2.Price - pp.low1.Price < 0 ?
                                        (pp.high2.Price - pp.low1.Price) * -1 :
                                        pp.high2.Price - pp.low1.Price;
                        double downTarget = (dp.PercentProfitTargetFibo / 100) * nDown;

                        downTarget -= pp.high2.Price;
                        downTarget *= -1; // Inverte valor negativo para positivo

                        Draw.Line(dp, dp.CurrentBar.ToString(), false, Miscellaneous.ConvertBarIndexToBarsAgo(dp, pp.low1.BarIndex),
                            downTarget, endLine, downTarget, Brushes.Red, DashStyleHelper.Solid, 2);

                        dp.ProfitTargetPriceSignal[0] = downTarget;
                        break;
                    case TrendDir.Up:
                        double nUp = pp.low2.Price - pp.high1.Price < 0 ?
                                        (pp.low2.Price - pp.high1.Price) * -1 :
                                        pp.low2.Price - pp.high1.Price;
                        double upTarget = (dp.PercentProfitTargetFibo / 100) * nUp;

                        upTarget += pp.low2.Price;

                        Draw.Line(dp, dp.CurrentBar.ToString(), false, Miscellaneous.ConvertBarIndexToBarsAgo(dp, pp.high1.BarIndex),
                            upTarget, endLine, upTarget, Brushes.Green, DashStyleHelper.Solid, 2);

                        dp.ProfitTargetPriceSignal[0] = upTarget;
                        break;
                    case TrendDir.Unknown:
                        dp.ProfitTargetPriceSignal[0] = dp.Input[0];
                        break;
                }
            }
        }
        #endregion
    }

    #region Data classes
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

        public HighLowPoint(DowPivot dp, double price, int barIndex, int pointIndex, TrendDir trendDir)
        {
            this.Price = price;
            this.BarIndex = barIndex;
            this.PointIndex = pointIndex;
            this.trendDir = trendDir;
            PrintPoint(dp);
        }
        #endregion

        public void Update(DowPivot dp, double price, int barIndex)
        {
            this.Price = price;
            this.BarIndex = barIndex;
            PrintPoint(dp);
        }
        private void PrintPoint(DowPivot dp)
        {
            if (!dp.DrawProp.ShowTopBottomPoints)
                return;
            switch (trendDir)
            {
                case TrendDir.Down:
                    Draw.Dot(dp, (trendDir + " Dot " + PointIndex.ToString()), false,
                                Miscellaneous.ConvertBarIndexToBarsAgo(dp, BarIndex), Price, Brushes.Red).OutlineBrush = Brushes.Transparent;
                    break;
                case TrendDir.Up:
                    Draw.Dot(dp, (trendDir + " Dot " + PointIndex.ToString()), false,
                                Miscellaneous.ConvertBarIndexToBarsAgo(dp, BarIndex), Price, Brushes.Green).OutlineBrush = Brushes.Transparent;
                    break;
            }

        }
    }

    public abstract class ZigZagDP : IZigZagBasicFunctions
    {
        private readonly List<HighLowPoint> lows;
        private readonly List<HighLowPoint> highs;
        private string lowTagName;
        private string highTagName;

        protected ZigZagDP(DowPivot dp)
        {
            lows = new List<HighLowPoint>();
            highs = new List<HighLowPoint>();

            //Inicia lista de HLPoints adcionando 4 novos objetos
            AddLow(dp, dp.Input[0], dp.CurrentBar);
            AddLow(dp, dp.Input[0], dp.CurrentBar);
            AddHigh(dp, dp.Input[0], dp.CurrentBar);
            AddHigh(dp, dp.Input[0], dp.CurrentBar);
        }

        #region GETs
        public HighLowPoint GetLow(int pointsAgo)
        {
            return lows[lows.Count - 1 - pointsAgo];
        }
        public HighLowPoint GetHigh(int pointsAgo)
        {
            return highs[highs.Count - 1 - pointsAgo];
        }
        #endregion

        #region SETs
        protected void AddLow(DowPivot dp, double price, int barIndex)
        {
            lows.Add(new HighLowPoint(dp, price, barIndex, lows.Count, TrendDir.Down));
            PrintZigZagLines(dp, Situation.AddLow);
        }
        protected void AddHigh(DowPivot dp, double price, int barIndex)
        {
            highs.Add(new HighLowPoint(dp, price, barIndex, highs.Count, TrendDir.Up));
            PrintZigZagLines(dp, Situation.AddHigh);
        }
        protected void UpdateLow(DowPivot dp, double price, int barIndex)
        {
            lows[lows.Count - 1].Update(dp, price, barIndex);
            PrintZigZagLines(dp, Situation.UpdateLow);
        }
        protected void UpdateHigh(DowPivot dp, double price, int barIndex)
        {
            highs[highs.Count - 1].Update(dp, price, barIndex);
            PrintZigZagLines(dp, Situation.UpdateHigh);
        }
        #endregion

        private void PrintZigZagLines(DowPivot dp, Situation situation)
        {
            if (lows.Count > 2 && highs.Count > 2 && dp.DrawProp.ShowZigZag)
            {
                switch (situation)
                {
                    case Situation.AddLow:
                        lowTagName = "Low line" + dp.CurrentBar;
                        Draw.Line(dp, lowTagName, false,
                            Miscellaneous.ConvertBarIndexToBarsAgo(dp, GetHigh(0).BarIndex), GetHigh(0).Price,
                            Miscellaneous.ConvertBarIndexToBarsAgo(dp, GetLow(0).BarIndex), GetLow(0).Price,
                            dp.DrawProp.ZigZagColor, DashStyleHelper.Solid, dp.DrawProp.ZigZagWidth);
                        break;
                    case Situation.AddHigh:
                        highTagName = "High line" + dp.CurrentBar;
                        Draw.Line(dp, highTagName, false,
                            Miscellaneous.ConvertBarIndexToBarsAgo(dp, GetLow(0).BarIndex), GetLow(0).Price,
                            Miscellaneous.ConvertBarIndexToBarsAgo(dp, GetHigh(0).BarIndex), GetHigh(0).Price,
                            dp.DrawProp.ZigZagColor, DashStyleHelper.Solid, dp.DrawProp.ZigZagWidth);
                        break;
                    case Situation.UpdateLow:
                        Draw.Line(dp, lowTagName, false,
                            Miscellaneous.ConvertBarIndexToBarsAgo(dp, GetHigh(0).BarIndex), GetHigh(0).Price,
                            Miscellaneous.ConvertBarIndexToBarsAgo(dp, GetLow(0).BarIndex), GetLow(0).Price,
                            dp.DrawProp.ZigZagColor, DashStyleHelper.Solid, dp.DrawProp.ZigZagWidth);
                        break;
                    case Situation.UpdateHigh:
                        Draw.Line(dp, highTagName, false,
                            Miscellaneous.ConvertBarIndexToBarsAgo(dp, GetLow(0).BarIndex), GetLow(0).Price,
                            Miscellaneous.ConvertBarIndexToBarsAgo(dp, GetHigh(0).BarIndex), GetHigh(0).Price,
                            dp.DrawProp.ZigZagColor, DashStyleHelper.Solid, dp.DrawProp.ZigZagWidth);
                        break;
                }
            }
        }

        public abstract void Calculate(DowPivot dp);

        /// <summary>
        /// This method tells you which trend leg is in formation at the moment.
        /// </summary>
        /// <returns></returns>
        public abstract TrendDir GetCurrentHighLowLeg();
    }

    public interface IZigZagBasicFunctions
    {
        void Calculate(DowPivot dp);
        TrendDir GetCurrentHighLowLeg();
    }
    public class PivotPoint
    {
        public HighLowPoint low1 { get; set; }
        public HighLowPoint low2 { get; set; }
        public HighLowPoint high1 { get; set; }
        public HighLowPoint high2 { get; set; }

        public PivotPoint()
        {
            this.low1 = new HighLowPoint();
            this.low2 = new HighLowPoint();
            this.high1 = new HighLowPoint();
            this.high2 = new HighLowPoint();
        }

        public PivotPoint(HighLowPoint low1, HighLowPoint low2, HighLowPoint high1, HighLowPoint high2)
        {
            this.low1 = new HighLowPoint(low1);
            this.low2 = new HighLowPoint(low2);
            this.high1 = new HighLowPoint(high1);
            this.high2 = new HighLowPoint(high2);
        }
    }
    #endregion

    public static class Miscellaneous
    {
        public static double GetCloseOrLow(DowPivot dp)
        {
            return dp.UseHighLow ? dp.Low[0] : dp.Close[0];
        }
        public static double GetClosrOrHigh(DowPivot dp)
        {
            return dp.UseHighLow ? dp.High[0] : dp.Close[0];
        }

        // Convert number index bar to bars ago
        public static int ConvertBarIndexToBarsAgo(DowPivot dp, int barIndex)
        {
            return ((barIndex - dp.CurrentBar) < 0) ? ((barIndex - dp.CurrentBar) * -1) : (barIndex - dp.CurrentBar);
        }
    }

    #region ENUMs
    public enum Situation
    {
        AddLow,
        AddHigh,
        UpdateLow,
        UpdateHigh
    };
    public enum TrendDir
    {
        Up,
        Down,
        Unknown
    };
    public enum ZigZagCalculationType
    {
        Points,
        Swing
    }
    #endregion
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DowPivot[] cacheDowPivot;
		public DowPivot DowPivot()
		{
			return DowPivot(Input);
		}

		public DowPivot DowPivot(ISeries<double> input)
		{
			if (cacheDowPivot != null)
				for (int idx = 0; idx < cacheDowPivot.Length; idx++)
					if (cacheDowPivot[idx] != null &&  cacheDowPivot[idx].EqualsInput(input))
						return cacheDowPivot[idx];
			return CacheIndicator<DowPivot>(new DowPivot(), input, ref cacheDowPivot);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DowPivot DowPivot()
		{
			return indicator.DowPivot(Input);
		}

		public Indicators.DowPivot DowPivot(ISeries<double> input )
		{
			return indicator.DowPivot(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DowPivot DowPivot()
		{
			return indicator.DowPivot(Input);
		}

		public Indicators.DowPivot DowPivot(ISeries<double> input )
		{
			return indicator.DowPivot(input);
		}
	}
}

#endregion
