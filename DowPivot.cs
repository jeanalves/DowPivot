#region Using declarations
using NinjaTrader.Custom.Indicators.DowPivotBase;
using NinjaTrader.Gui;
using System;
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
        private PointsCalculation pointsCalculation;
        private SwingDelayedCalculation swingDelayedCalculation;
        private SwingForwardCalculationOld swingForwardCalculationOld;
        private SwingForwardCalculationNew swingForwardCalculationNew;
        private PivotLogic pivotLogic;

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
                CalculationType = ZigZagCalculationType.SwingDelayed;
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
                pointsCalculation = new PointsCalculation(this);
                swingDelayedCalculation = new SwingDelayedCalculation(this);
                swingForwardCalculationOld = new SwingForwardCalculationOld(this);
                swingForwardCalculationNew = new SwingForwardCalculationNew(this);
                pivotLogic = new PivotLogic(this);

                // Toda vez que a tecla F5 for pressionada automaticamente passara pelo metodo
                // NinjaTrader.Code.Output.Reset() e limpara todas as abas da janela Output das saidas anteriores.
                NinjaTrader.Code.Output.Reset(PrintTo.OutputTab1);
                NinjaTrader.Code.Output.Reset(PrintTo.OutputTab2);
            }
        }

        protected override void OnBarUpdate()
        {
            //if (CurrentBar < 20)
                //return;

            try
            {
                switch (CalculationType)
                {
                    case ZigZagCalculationType.Points:
                        pointsCalculation.Calculate(this);
                        pivotLogic.Calculate(this, pointsCalculation);
                        break;
                    case ZigZagCalculationType.SwingDelayed:
                        swingDelayedCalculation.Calculate(this);
                        pivotLogic.Calculate(this, swingDelayedCalculation);
                        break;
                    case ZigZagCalculationType.SwingForwardOld:
                        swingForwardCalculationOld.Calculate(this);
                        pivotLogic.Calculate(this,swingForwardCalculationOld);
                        break;
                    case ZigZagCalculationType.SwingForwardNew:
                        swingForwardCalculationNew.Calculate(this);
                        pivotLogic.Calculate(this, swingForwardCalculationNew);
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

        #region Zig Zag parameters
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
        #endregion

        #region Pivot parameters
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
        #endregion

        #endregion
    }
}

#region Sub menu's class
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

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DowPivot[] cacheDowPivot;
		public DowPivot DowPivot(ZigZagCalculationType calculationType, double strength, bool useHighLow, DrawProperties drawProp, double percentProfitTargetFibo, FiboPivotRetraction fiboPivot)
		{
			return DowPivot(Input, calculationType, strength, useHighLow, drawProp, percentProfitTargetFibo, fiboPivot);
		}

		public DowPivot DowPivot(ISeries<double> input, ZigZagCalculationType calculationType, double strength, bool useHighLow, DrawProperties drawProp, double percentProfitTargetFibo, FiboPivotRetraction fiboPivot)
		{
			if (cacheDowPivot != null)
				for (int idx = 0; idx < cacheDowPivot.Length; idx++)
					if (cacheDowPivot[idx] != null && cacheDowPivot[idx].CalculationType == calculationType && cacheDowPivot[idx].Strength == strength && cacheDowPivot[idx].UseHighLow == useHighLow && cacheDowPivot[idx].DrawProp == drawProp && cacheDowPivot[idx].PercentProfitTargetFibo == percentProfitTargetFibo && cacheDowPivot[idx].FiboPivot == fiboPivot && cacheDowPivot[idx].EqualsInput(input))
						return cacheDowPivot[idx];
			return CacheIndicator<DowPivot>(new DowPivot(){ CalculationType = calculationType, Strength = strength, UseHighLow = useHighLow, DrawProp = drawProp, PercentProfitTargetFibo = percentProfitTargetFibo, FiboPivot = fiboPivot }, input, ref cacheDowPivot);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DowPivot DowPivot(ZigZagCalculationType calculationType, double strength, bool useHighLow, DrawProperties drawProp, double percentProfitTargetFibo, FiboPivotRetraction fiboPivot)
		{
			return indicator.DowPivot(Input, calculationType, strength, useHighLow, drawProp, percentProfitTargetFibo, fiboPivot);
		}

		public Indicators.DowPivot DowPivot(ISeries<double> input , ZigZagCalculationType calculationType, double strength, bool useHighLow, DrawProperties drawProp, double percentProfitTargetFibo, FiboPivotRetraction fiboPivot)
		{
			return indicator.DowPivot(input, calculationType, strength, useHighLow, drawProp, percentProfitTargetFibo, fiboPivot);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DowPivot DowPivot(ZigZagCalculationType calculationType, double strength, bool useHighLow, DrawProperties drawProp, double percentProfitTargetFibo, FiboPivotRetraction fiboPivot)
		{
			return indicator.DowPivot(Input, calculationType, strength, useHighLow, drawProp, percentProfitTargetFibo, fiboPivot);
		}

		public Indicators.DowPivot DowPivot(ISeries<double> input , ZigZagCalculationType calculationType, double strength, bool useHighLow, DrawProperties drawProp, double percentProfitTargetFibo, FiboPivotRetraction fiboPivot)
		{
			return indicator.DowPivot(input, calculationType, strength, useHighLow, drawProp, percentProfitTargetFibo, fiboPivot);
		}
	}
}

#endregion
