using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Windows.Media;

namespace NinjaTrader.Custom.Indicators.DowPivotBase
{
    public static class Miscellaneous
    {
        public static double GetCloseOrLow(DowPivot dowPivot)
        {
            return dowPivot.UseHighLow ? dowPivot.Low[0] : dowPivot.Close[0];
        }
        public static double GetClosrOrHigh(DowPivot dowPivot)
        {
            return dowPivot.UseHighLow ? dowPivot.High[0] : dowPivot.Close[0];
        }

        // Convert number index bar to bars ago
        public static int ConvertBarIndexToBarsAgo(DowPivot dowPivot, int barIndex)
        {
            return ((barIndex - dowPivot.CurrentBar) < 0) ? ((barIndex - dowPivot.CurrentBar) * -1) : (barIndex - dowPivot.CurrentBar);
        }

        // To debug code
        public static void DrawText(DowPivot dowPivot, string text, int barsAgo, double price, int yPixelOff)
        {
            Draw.Text(dowPivot, "Miscellaneous debug " + dowPivot.CurrentBar + " " + text, false, text, barsAgo, price, yPixelOff, Brushes.White,
                new Gui.Tools.SimpleFont("Arial", 11), System.Windows.TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 100);
        }

        public static void PrintError(DowPivot dowPivot, string text)
        {
            Draw.TextFixed(dowPivot, "Error debug", text, TextPosition.BottomRight);
        }

        public static void PrintTab2(object text)
        {
            NinjaTrader.Code.Output.Process(text.ToString(), PrintTo.OutputTab2);
        }

        public static void PrintStatusOfObjects(ArrayList arrayList)
        {
            string text = "";

            for(int i = 0; i < arrayList.Count; i++)
            {
                text += GetMemberName(() => arrayList[i]) + "    " + arrayList[i];
            }

            PrintTab2(text);
        }

        public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
        {
            if (memberExpression is MemberExpression)
            {
                MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
                return expressionBody.Member.Name;
            }

            return null;
        }
    }
}
