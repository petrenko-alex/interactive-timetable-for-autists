using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.Droid.ApplicationLayer;
using MikePhil.Charting.Charts;
using MikePhil.Charting.Components;
using MikePhil.Charting.Data;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    public class GraphDialogFragment : DialogFragment
    {
        #region Constants
        public static readonly string FragmentTag = "graph_dialog_fragment";
        private static readonly string DateFormat = "dd.MM.yyyy";
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

            /* Prepare and set data */
            var entries = new List<Entry>();
            int amountOfDiagnosticResults = _diagnosticResults.Count;
            for (int i = 0; i < amountOfDiagnosticResults; ++i)
            {
                var entry = new Entry(i,_diagnosticResults[i]);
                entries.Add(entry);
            }

            var dataSet = new LineDataSet(entries, "Monitoring");
            var color = ContextCompat.GetColor(Activity, Resource.Color.theme_accent_color);
            dataSet.HighLightColor = color;             
                

            var lineData = new LineData(dataSet);
            lineData.SetValueFormatter(new GraphValueFormatter());
            _graph.Data = lineData;

            /* Adjust xAxis */
            var dates = _dates.Select(x => x.ToString(DateFormat)).ToList();

            var xAxis = _graph.XAxis;
            xAxis.ValueFormatter = new GraphAxisValueFormatter(dates);
            xAxis.Granularity = 1;
            xAxis.Position = XAxis.XAxisPosition.Bottom;
            xAxis.TextSize = 25;
            xAxis.AxisLineWidth = 3;

            var leftAxis = _graph.AxisLeft;
            leftAxis.TextSize = 20;
            leftAxis.AxisLineWidth = 3;
            leftAxis.SpaceTop = 10;
            leftAxis.SpaceBottom = 10;

            var rightAxis = _graph.AxisRight;
            rightAxis.TextSize = 20;
            rightAxis.AxisLineWidth = 3;
            rightAxis.SpaceTop = 10;
            rightAxis.SpaceBottom = 10;

            _graph.SetDrawBorders(true);
            _graph.Legend.Enabled = false;
            _graph.SetExtraOffsets(30,10,30,10);

            /* Refresh _graph */
            _graph.AnimateY(1000);

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

        #endregion



    }
}