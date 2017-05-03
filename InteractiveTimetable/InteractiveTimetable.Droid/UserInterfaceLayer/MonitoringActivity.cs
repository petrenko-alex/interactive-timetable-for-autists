using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.Droid.ApplicationLayer;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "MonitoringActivity", MainLauncher = true)]
    public class MonitoringActivity : ActionBarActivity
    {
        #region Constants
        private static readonly int HeaderColumnWidth = 150;
        private static readonly int HeaderColumnHeight = 50;

        #endregion

        #region Widgets
        private HorizontalScrollView _layoutForTable;
        private ImageButton _backButton;
        private ImageButton _homeButton;
        private TableLayout _table;        
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.monitoring);
            OverridePendingTransition(
                Resource.Animation.enter_from_right,
                Resource.Animation.exit_to_left
            );

            /* Get views */
            var tripInfo = FindViewById<TextView>(Resource.Id.monitoring_trip_info);
            var heading = FindViewById<TextView>(Resource.Id.monitoring_header);
            _layoutForTable = FindViewById<HorizontalScrollView>(Resource.Id.table_horizontal_scroll);

            /* Get data */
            var userId = Intent.GetIntExtra("user_id", 0);
            var tripId = Intent.GetIntExtra("trip_id", 0);

            // TODO: Delete after finish
            userId = 1;
            tripId = 1;

            /* Set data to views */
            if (tripId > 0)
            {
                var trip = InteractiveTimetable.Current.HospitalTripManager.GetHospitalTrip(tripId);
                string text = $"{GetString(Resource.String.trip_in_list)}" +
                              $"{trip.Number} " +
                              $"{trip.StartDate:dd.MM.yyyy} - {trip.FinishDate:dd.MM.yyyy}";
                tripInfo.Text = text;

                heading.Text = GetString(Resource.String.trip_monitoring);
            }
            else
            {
                tripInfo.Visibility = ViewStates.Gone;
                heading.Text = GetString(Resource.String.general_monitoring);
            }

            if (userId > 0)
            {
                var user = InteractiveTimetable.Current.UserManager.GetUser(userId);
                heading.Text = $"{user.FirstName} - {heading.Text}";
            }

            /* Set tool bar */
            var toolbar = FindViewById<Toolbar>(Resource.Id.monitoring_toolbar);
            SetSupportActionBar(toolbar);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            AdjustToolbarForActivity();
        }

        private void AdjustToolbarForActivity()
        {
            /* Set toolbar layout */
            var toolbar = FindViewById<Toolbar>(Resource.Id.monitoring_toolbar);
            var toolbarContent = FindViewById<LinearLayout>(Resource.Id.toolbar_content);
            var layout = LayoutInflater.Inflate(Resource.Layout.monitoring_toolbar, toolbar, false);
            toolbarContent.AddView(layout);

            /* Set toolbar controls */
            var title = toolbar.FindViewById<TextView>(Resource.Id.toolbar_title);
            title.Text = GetString(Resource.String.monitoring);

            var clock = toolbar.FindViewById<TextClock>(Resource.Id.toolbar_clock);
            clock.Format24Hour = InteractiveTimetable.DateTimeFormat;

            _backButton = toolbar.FindViewById<ImageButton>(Resource.Id.toolbar_back);
            _backButton.Click += OnBackButtonClicked;

            _homeButton = toolbar.FindViewById<ImageButton>(Resource.Id.toolbar_home);
            _homeButton.Click += OnHomeButtonClicked;

            CreateTable();
        }

        private void CreateTable()
        {
            /* Get data for table */

            /* Create table */   
            _table = new TableLayout(this);
            /*_table.LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent
            );*/

            /* Create header */
            AddHeaderToTable(_table);
            
            /* Create rows for grades */
            /* Create rows for sums */


            _layoutForTable.AddView(_table);
        }

        private void OnHomeButtonClicked(object sender, EventArgs e)
        {
            Finish();

            /* Call home screen activity */
            var intent = new Intent(this, typeof(HomeScreenActivity));
            intent.SetFlags(ActivityFlags.ClearTop);
            StartActivity(intent);
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
        {
            Finish();

            OverridePendingTransition(
                Resource.Animation.enter_from_left,
                Resource.Animation.exit_to_right
            );
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();

            OverridePendingTransition(
                Resource.Animation.enter_from_left,
                Resource.Animation.exit_to_right
            );
        }

        #region Table Methods

        private void AddHeaderToTable(TableLayout table)
        {
            /* Get data */
            var criterions = InteractiveTimetable.Current.DiagnosticManager.GetCriterions().ToList();

            /* Prepare layout params */
            var paramsForDefinitions = new TableRow.LayoutParams(
                HeaderColumnWidth,
                HeaderColumnHeight
            );

            /* Create and insert first header column */
            var topLeftColumn = CreateColumn(
                paramsForDefinitions,
                GetString(Resource.String.criterions_and_grades)
            );
            topLeftColumn.Gravity = GravityFlags.Center;

            var firstRow = CreateRow();
            firstRow.AddView(topLeftColumn);
            table.AddView(firstRow);

            /* Create and insert second header column */
            var dateColumn = CreateColumn(
                paramsForDefinitions,
                GetString(Resource.String.date)
            );

            var secondRow = CreateRow();
            secondRow.AddView(dateColumn);
            table.AddView(secondRow);

            /* Add criterion definition header columns */
            foreach (var criterion in criterions)
            {
                /* Create column for criterion definition */
                var column = CreateColumn(paramsForDefinitions, criterion);

                /* Add to table */
                var row = CreateRow();
                row.AddView(column);
                _table.AddView(row);
            }

            /* Add columns for sums */
            var partialSumsColumns = CreateColumn(
                paramsForDefinitions,
                GetString(Resource.String.grade_partial_sum)
            );
            var sumColumn = CreateColumn(
                paramsForDefinitions,
                GetString(Resource.String.grade_total_sum)
            );

            var lastRow1 = CreateRow();
            lastRow1.AddView(partialSumsColumns);
            table.AddView(lastRow1);

            var lastRow2 = CreateRow();
            lastRow2.AddView(sumColumn);
            table.AddView(lastRow2);
        }

        private TableRow CreateRow()
        {
            var tableParams = new TableLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent
            );

            var row = new TableRow(this)
            {
                LayoutParameters = tableParams
            };

            return row;
        }

        private TextView CreateColumn(TableRow.LayoutParams layoutParams, string columnText)
        {
            /* Create column with parameters */
            var column = new TextView(this)
            {
                LayoutParameters = layoutParams,
            };
            column.SetBackgroundResource(Resource.Drawable.table_frame);

            var paddingInDp = ImageHelper.ConvertPixelsToDp(5);
            column.SetPadding(paddingInDp, 0, paddingInDp, 0);
            column.Gravity = GravityFlags.CenterVertical;

            /* Set text */
            column.Text = columnText;

            return column;
        }

        #endregion
    }
}