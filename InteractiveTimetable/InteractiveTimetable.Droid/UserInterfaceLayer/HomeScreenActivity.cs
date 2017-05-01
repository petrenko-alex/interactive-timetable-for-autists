using System;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "HomeScreenActivity")]
    public class HomeScreenActivity : ActionBarActivity
    {
        #region Widgets

        private TextView _greetingsString;
        private FrameLayout _timetablePartButton;
        private FrameLayout _managementPartButton;
        #endregion


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.home_screen);

            /* Set tool bar */
            var toolbar = FindViewById<Toolbar>(Resource.Id.hs_toolbar);
            SetSupportActionBar(toolbar);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            AdjustToolbarForActivity();

            /* Get data */
            var userName = Intent.GetStringExtra("user_name");

            /* Get views */
            _greetingsString = FindViewById<TextView>(Resource.Id.hs_welcome);
            _timetablePartButton = FindViewById<FrameLayout>(Resource.Id.timetable_part_frame);
            _managementPartButton = FindViewById<FrameLayout>(Resource.Id.management_part_frame);

            /* Set widgets data */
            _greetingsString.Text += $", {userName}!";

            /* Set handlers */
            _timetablePartButton.Click += OnTimetablePartButtonClicked;
            _managementPartButton.Click += OnManagementPartButtonClicked;
        }

        private void OnManagementPartButtonClicked(object sender, EventArgs e)
        {
            Console.WriteLine("Management part");
        }

        private void OnTimetablePartButtonClicked(object sender, EventArgs e)
        {
            Console.WriteLine("Timetable part");
        }

        private void AdjustToolbarForActivity()
        {
            /* Set toolbar layout */
            var toolbar = FindViewById<Toolbar>(Resource.Id.hs_toolbar);
            var toolbarContent = FindViewById<LinearLayout>(Resource.Id.toolbar_content);
            var layout = LayoutInflater.Inflate(Resource.Layout.home_screen_toolbar, toolbar, false);
            toolbarContent.AddView(layout);

            /* Set toolbar controls */
            var title = toolbar.FindViewById<TextView>(Resource.Id.toolbar_title);
            title.Text = GetString(Resource.String.home_screen);

            var clock = toolbar.FindViewById<TextClock>(Resource.Id.toolbar_clock);
            clock.Format24Hour = InteractiveTimetable.DateTimeFormat;

            var logoutButton = toolbar.FindViewById<ImageButton>(Resource.Id.toolbar_logout);
            logoutButton.Click += OnLogoutButtonClicked;
        }

        private void OnLogoutButtonClicked(object sender, EventArgs e)
        {
            /* Finish activity */
            Finish();
        }
    }
}