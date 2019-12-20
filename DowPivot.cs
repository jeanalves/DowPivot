#region Using declarations
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Custom.Indicators.DowPivotBase;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.DrawingTools;
using System;
using System.Collections;
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
        private SwingCalculation swingCalculation;
        private PointsCalculation pointsCalculation;
        private PivotPointsLogic pivotPointsLogic;

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
                swingCalculation = new SwingCalculation(this);
                pointsCalculation = new PointsCalculation(this);
                pivotPointsLogic = new PivotPointsLogic(this);

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
                        swingCalculation.Calculate(this);
                        pivotPointsLogic.Calculate(this, swingCalculation);
                        break;
                    case ZigZagCalculationType.Points:
                        pointsCalculation.Calculate(this);
                        pivotPointsLogic.Calculate(this, pointsCalculation);
                        break;
                }
            }
            catch (Exception e)
            {
                Print(e.ToString());
                Print("Current bar: " + CurrentBar);
            }
        }
        // Test
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
        //[NinjaScriptProperty]
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
        //[NinjaScriptProperty]
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
        //[NinjaScriptProperty]
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

    public class SwingCalculation : DowPivotZigZag
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

        public SwingCalculation(DowPivot dowPivot) : base(dowPivot)
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
                ArrayList lastLowCacheTick = new ArrayList(lastLowCache);
                ArrayList lastHighCacheTick = new ArrayList(lastHighCache);

                lastLowCacheTick[lastLowCacheTick.Count - 1] = dowPivot.Low[0];
                lastHighCacheTick[lastHighCacheTick.Count - 1] = dowPivot.High[0];

                // Low calculations
                bool isSwingLowTick = true;
                double swingLowCandidateValueTick = (double)lastLowCacheTick[strength];

                for (int i = 0; i < dowPivot.Strength; i++)
                    if (((double)lastLowCache[i]).ApproxCompare(swingLowCandidateValueTick) <= 0)
                        isSwingLowTick = false;

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

    public class PointsCalculation : DowPivotZigZag
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

        public PointsCalculation(DowPivot dowPivot) : base(dowPivot)
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

    public class PivotPointsLogic
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

        public PivotPointsLogic(DowPivot dowPivot)
        {
            SetStopLossPrice(dowPivot, dowPivot.Input[0]);                      // Initiate
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

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DowPivot[] cacheDowPivot;
		public DowPivot DowPivot(double strength, bool useHighLow, double percentProfitTargetFibo)
		{
			return DowPivot(Input, strength, useHighLow, percentProfitTargetFibo);
		}

		public DowPivot DowPivot(ISeries<double> input, double strength, bool useHighLow, double percentProfitTargetFibo)
		{
			if (cacheDowPivot != null)
				for (int idx = 0; idx < cacheDowPivot.Length; idx++)
					if (cacheDowPivot[idx] != null && cacheDowPivot[idx].Strength == strength && cacheDowPivot[idx].UseHighLow == useHighLow && cacheDowPivot[idx].PercentProfitTargetFibo == percentProfitTargetFibo && cacheDowPivot[idx].EqualsInput(input))
						return cacheDowPivot[idx];
			return CacheIndicator<DowPivot>(new DowPivot(){ Strength = strength, UseHighLow = useHighLow, PercentProfitTargetFibo = percentProfitTargetFibo }, input, ref cacheDowPivot);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DowPivot DowPivot(double strength, bool useHighLow, double percentProfitTargetFibo)
		{
			return indicator.DowPivot(Input, strength, useHighLow, percentProfitTargetFibo);
		}

		public Indicators.DowPivot DowPivot(ISeries<double> input , double strength, bool useHighLow, double percentProfitTargetFibo)
		{
			return indicator.DowPivot(input, strength, useHighLow, percentProfitTargetFibo);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DowPivot DowPivot(double strength, bool useHighLow, double percentProfitTargetFibo)
		{
			return indicator.DowPivot(Input, strength, useHighLow, percentProfitTargetFibo);
		}

		public Indicators.DowPivot DowPivot(ISeries<double> input , double strength, bool useHighLow, double percentProfitTargetFibo)
		{
			return indicator.DowPivot(input, strength, useHighLow, percentProfitTargetFibo);
		}
	}
}

#endregion
