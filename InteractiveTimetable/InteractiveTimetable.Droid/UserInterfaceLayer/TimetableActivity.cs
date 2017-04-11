using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.Droid.ApplicationLayer;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "Interactive Timetable", MainLauncher = true/*, Theme = "@android:style/Theme.Holo.Light"*/)]
    public class TimetableActivity : Activity, ViewTreeObserver.IOnGlobalLayoutListener
    {
        #region Constants
        private static readonly string DateTimeFormat = "d MMMM yyyy, EEEE   k:mm";
        #endregion

        #region Widgets
        private LinearLayout _mainLayout;
        private LinearLayout _timetableTapeLayout;
        private TextClock _clock;
        private ImageButton _managementPanelButton;
        private ImageButton _lockScreenButton;
        #endregion

        #region Flags
        private bool _isLocked;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.timetable);

            /* Hide action bar */
            ActionBar.Hide();

            /* Get layouts */
            _mainLayout = FindViewById<LinearLayout>(Resource.Id.timetable_main_layout);
            _timetableTapeLayout = FindViewById<LinearLayout>(Resource.Id.timetable_tape_layout);

            /* Set management panel button */
            _managementPanelButton = FindViewById<ImageButton>(Resource.Id.management_panel_button);
            _managementPanelButton.Click += OnManagementPanelButtonClicked;

            /* Set clock */
            _clock = FindViewById<TextClock>(Resource.Id.clock);
            _clock.Format24Hour = DateTimeFormat;

            /* Set lock screen button */
            _lockScreenButton = FindViewById<ImageButton>(Resource.Id.lock_screen_button);
            _lockScreenButton.Click += OnLockScreenButtonClicked;

            /* Add timetable tapes */
            /* Get users who has current hospital trips */
            //var userSchedules = user.Schedules;
            //var currentSchedule = userSchedules.Any()
             //   ? userSchedules.OrderByDescending(x => x.CreateTime).FirstOrDefault() : null;

            /* Get user schedule */

            _mainLayout.ViewTreeObserver.AddOnGlobalLayoutListener(this);
        }

        private void OnManagementPanelButtonClicked(object sender, EventArgs e)
        {
            /* Start management activity */
            var intent = new Intent(this, typeof(ManagementActivity));
            StartActivity(intent);
        }

        private void OnLockScreenButtonClicked(object sender, EventArgs e)
        {
            /* If locked, need to unlock */
            if (_isLocked)
            {
                /* Reset handler for locked screen */
                _managementPanelButton.Click -= OnLockedScreenClicked;
                _mainLayout.Click -= OnLockedScreenClicked;

                /* Set function handlers*/
                _managementPanelButton.Click += OnManagementPanelButtonClicked;

                /* Set button image */
                _lockScreenButton.SetImageResource(Resource.Drawable.unlocked_icon);

                _isLocked = false;
            }
            /* If unlocked, need to lock */
            else
            {
                /* Reset function handlers */
                _managementPanelButton.Click -= OnManagementPanelButtonClicked;

                /* Set handler for lock screen */
                _managementPanelButton.Click += OnLockedScreenClicked;
                _mainLayout.Click += OnLockedScreenClicked;

                /* Set button image */
                _lockScreenButton.SetImageResource(Resource.Drawable.locked_icon);

                _isLocked = true;
            }
        }

        private void OnLockedScreenClicked(object sender, EventArgs e)
        {
            var toast = ToastHelper.GetInfoToast(this, GetString(Resource.String.screen_is_locked));
            toast.Show();
        }

        public void OnGlobalLayout()
        {
            // TODO: Delete method and class extend when don't need to know layout sizes
            /*/* _mainLayout size in px #1#
            int widthPx1 = _mainLayout.Width;
            int heightPx1 = _mainLayout.Height;

            Console.WriteLine($"Layout Width:{widthPx1} px");
            Console.WriteLine($"Layout Height:{heightPx1} px");

            /* _mainLayout size in dp #1#
            var displayMetrics = Resources.DisplayMetrics;
            float dpHeight1 = heightPx1 / displayMetrics.Density;
            float dpWidth1 = widthPx1 / displayMetrics.Density;

            Console.WriteLine($"Layout Width:{dpWidth1} dp");
            Console.WriteLine($"Layout Height:{dpHeight1} dp");*/

            /* _timetableTapeLayout size in px */
            int widthPx = _timetableTapeLayout.Width;
            int heightPx = _timetableTapeLayout.Height;

            Console.WriteLine($"Layout Width:{widthPx} px");
            Console.WriteLine($"Layout Height:{heightPx} px");

            /* _timetableTapeLayout size in dp */
            var displayMetrics = Resources.DisplayMetrics;
            float dpHeight = heightPx / displayMetrics.Density;
            float dpWidth = widthPx / displayMetrics.Density;

            Console.WriteLine($"Layout Width:{dpWidth} dp");
            Console.WriteLine($"Layout Height:{dpHeight} dp");

            _mainLayout.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);
        }
    }
}