using MikePhil.Charting.Data;
using MikePhil.Charting.Formatter;
using MikePhil.Charting.Util;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    public class GraphValueFormatter : Java.Lang.Object, IValueFormatter
    {
        public string GetFormattedValue(float value, Entry entry, int dataSetIndex, ViewPortHandler viewPortHandler)
        {
            return value.ToString("####") + " баллов";
        }
    }
}