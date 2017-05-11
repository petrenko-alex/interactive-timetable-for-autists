using System.Collections.Generic;
using MikePhil.Charting.Components;
using MikePhil.Charting.Formatter;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    public class GraphAxisValueFormatter : Java.Lang.Object, IAxisValueFormatter
    {
        private List<string> _values;

        public GraphAxisValueFormatter(List<string> values)
        {
            _values = values;
        }

        public string GetFormattedValue(float value, AxisBase axis)
        {
            return _values[(int) value];
        }
    }
}