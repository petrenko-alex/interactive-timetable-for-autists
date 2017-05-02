using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "MonitoringActivity", MainLauncher = true)]
    public class MonitoringActivity : ActionBarActivity
    {
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
            var tableParams = new TableLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent
            );
            var rowParams = new TableRow.LayoutParams(
                150,
                50
            );

            _table = new TableLayout(this);
            /*_table.LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent
            );*/
            

            /* Create header */
            var headingRow = new TableRow(this);
            headingRow.LayoutParameters = tableParams;
            
            var mainHeaderColumn = new TextView(this);
            mainHeaderColumn.LayoutParameters = rowParams;
            mainHeaderColumn.Text = "Критерии и оценки";
            mainHeaderColumn.SetBackgroundResource(Resource.Drawable.table_frame);
            mainHeaderColumn.Gravity = GravityFlags.Center;

            headingRow.AddView(mainHeaderColumn);
            _table.AddView(headingRow);

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
    }
}