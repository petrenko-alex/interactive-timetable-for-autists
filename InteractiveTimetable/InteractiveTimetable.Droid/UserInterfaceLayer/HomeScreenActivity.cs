using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "HomeScreenActivity", LaunchMode = LaunchMode.SingleTask)]
    public class HomeScreenActivity : ActionBarActivity
    {
        #region Constants
        private static readonly string DateFormat = "dd.MM.yyyy";
        #endregion

        #region Widgets
        private TextView _greetingsString;
        private FrameLayout _timetablePartButton;
        private FrameLayout _managementPartButton;
        #endregion

        #region Internal Variables
        private string _currentUser;
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
            _currentUser = Intent.GetStringExtra("user_name");
            
            /* Get views */
            _greetingsString = FindViewById<TextView>(Resource.Id.hs_welcome);
            _timetablePartButton = FindViewById<FrameLayout>(Resource.Id.timetable_part_button);
            _managementPartButton = FindViewById<FrameLayout>(Resource.Id.management_part_button);

            /* Set widgets data */
            _greetingsString.Text += $", {_currentUser}!";

            /* Set handlers */
            _timetablePartButton.Click += OnTimetablePartButtonClicked;
            _managementPartButton.Click += OnManagementPartButtonClicked;
        }

        private void OnManagementPartButtonClicked(object sender, EventArgs e)
        {
            /* Call management activity */
            var intent = new Intent(this, typeof(ManagementActivity));
            StartActivity(intent);
        }

        private void OnTimetablePartButtonClicked(object sender, EventArgs e)
        {
            /* Ask for a date and call timetable activity */
            var chosenDate = DateTime.Today;

            var fragment = DatePickerFragment.NewInstance(
                chosenDate,
                StartTimetableActivity
            );
            fragment.Show(FragmentManager, DatePickerFragment.FragmentTag);
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
            Finish();
        }

        private void StartTimetableActivity(DateTime date)
        {
            var intent = new Intent(this, typeof(TimetableActivity));
            intent.PutExtra("date", date.ToString(DateFormat));
            intent.PutExtra("date_format", DateFormat);
            StartActivity(intent);
        }
    }
}