using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using MikePhil.Charting.Charts;
using MikePhil.Charting.Components;
using MikePhil.Charting.Data;
using MikePhil.Charting.Formatter;
using MikePhil.Charting.Util;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    public class GraphDialogFragment : DialogFragment
    {
        #region Constants
        public static readonly string FragmentTag = "graph_dialog_fragment";
        private static readonly string DateFormat = "dd.MM.yyyy";
        private static readonly string LineDataSetLabel = "Monitoring";
        private static readonly int TextSize = 25;
        private static readonly int XAxisStep = 1;
        private static readonly int AxisLineWidth = 3;
        private static readonly int SpacePercent = 10;
        private static readonly int HorizontalExtraOffset = 30;
        private static readonly int VerticalExtraOffset = 10;
        private static readonly int AnimationTime = 1000;
        #endregion

        #region Internal Variables
        private List<int> _diagnosticResults;
        private List<DateTime> _dates;
        #endregion

        #region Widgets
        private LineChart _graph;
        private Button _closeButton;
        #endregion

        #region Methods

        #region Construct Methods
        public static GraphDialogFragment NewInstance(List<int> results, List<DateTime> dates)
        {
            var fragment = new GraphDialogFragment()
            {
                _diagnosticResults = results,
                _dates = dates
            };

            return fragment;
        }
        #endregion

        #region Event Handlers
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.graph_dialog, container, false);

            /* Get views */
            _graph = view.FindViewById<LineChart>(Resource.Id.graph);
            _closeButton = view.FindViewById<Button>(Resource.Id.close_graph_button);

            /* Set handlers */
            _closeButton.Click += OnCloseButtonClicked;

            /* Set dialog params */
            Dialog.SetTitle(GetString(Resource.String.graph_title));

            SetupGraph();

            return view;
        }

        private void OnCloseButtonClicked(object sender, EventArgs e)
        {
            Dismiss();
        }

        public override void OnDestroy()
        {
            _dates.Clear();
            _diagnosticResults.Clear();
            _dates = null;
            _diagnosticResults = null;

            _closeButton.Click -= OnCloseButtonClicked;

            base.OnDestroy();
        }
        #endregion

        #region Other Methods
        private void SetupGraph()
        {
            /* Prepare data */
            var entries = new List<Entry>();
            int amountOfDiagnosticResults = _diagnosticResults.Count;
            for (int i = 0; i < amountOfDiagnosticResults; ++i)
            {
                var entry = new Entry(i, _diagnosticResults[i]);
                entries.Add(entry);
            }

            /* Set data */
            var dataSet = new LineDataSet(entries, LineDataSetLabel);
            var color = ContextCompat.GetColor(Activity, Resource.Color.theme_accent_color);
            dataSet.HighLightColor = color;

            var lineData = new LineData(dataSet);
            lineData.SetValueFormatter(new GraphValueFormatter(Activity));
            _graph.Data = lineData;

            /* Set X axis */
            var dates = _dates.Select(x => x.ToString(DateFormat)).ToList();

            var xAxis = _graph.XAxis;
            xAxis.ValueFormatter = new GraphAxisValueFormatter(dates);
            xAxis.Granularity = XAxisStep;
            xAxis.Position = XAxis.XAxisPosition.Bottom;
            xAxis.TextSize = TextSize;
            xAxis.AxisLineWidth = AxisLineWidth;

            /* Set left Y axis */
            var leftAxis = _graph.AxisLeft;
            leftAxis.TextSize = TextSize;
            leftAxis.AxisLineWidth = AxisLineWidth;
            leftAxis.SpaceTop = SpacePercent;
            leftAxis.SpaceBottom = SpacePercent;

            /* Set right Y axis */
            var rightAxis = _graph.AxisRight;
            rightAxis.TextSize = TextSize;
            rightAxis.AxisLineWidth = AxisLineWidth;
            rightAxis.SpaceTop = SpacePercent;
            rightAxis.SpaceBottom = SpacePercent;

            /* Set graph params */
            _graph.SetDrawBorders(true);
            _graph.Legend.Enabled = false;
            _graph.SetExtraOffsets(
                HorizontalExtraOffset,
                VerticalExtraOffset,
                HorizontalExtraOffset,
                VerticalExtraOffset
                );

            /* Refresh _graph */
            _graph.AnimateY(AnimationTime);
        }
        #endregion

        #endregion
    }

    public class GraphAxisValueFormatter : Java.Lang.Object, IAxisValueFormatter
    {
        private List<string> _values;

        public GraphAxisValueFormatter(List<string> values)
        {
            _values = values;
        }

        public string GetFormattedValue(float value, AxisBase axis)
        {
            return _values[(int)value];
        }
    }

    public class GraphValueFormatter : Java.Lang.Object, IValueFormatter
    {
        private static readonly string ValueFormat = "####";
        private Activity _context;

        public GraphValueFormatter(Activity context)
        {
            _context = context;
        }

        public string GetFormattedValue(float value, Entry entry, int dataSetIndex, ViewPortHandler viewPortHandler)
        {
            return value.ToString(ValueFormat) + " " + _context.GetString(Resource.String.points);
        }
    }
}