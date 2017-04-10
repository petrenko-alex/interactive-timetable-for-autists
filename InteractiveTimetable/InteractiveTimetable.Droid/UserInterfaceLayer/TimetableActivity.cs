using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.Droid.ApplicationLayer;
using Java.Lang;
using Java.Text;
using Java.Util;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "Timetable", MainLauncher = true/*, Theme = "@android:style/Theme.Holo.Light"*/)]
    public class TimetableActivity : Activity
    {
        #region Constants
        private static readonly string DateTimeFormat = "d MMMM yyyy, EEEE   k:mm";
        #endregion

        #region Widgets
        private LinearLayout _mainLayout;
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

            _mainLayout = FindViewById<LinearLayout>(Resource.Id.timetable_main_layout);

            /* Set management panel button */
            _managementPanelButton = FindViewById<ImageButton>(Resource.Id.management_panel_button);
            _managementPanelButton.Click += OnManagementPanelButtonClicked;

            /* Set clock */
            _clock = FindViewById<TextClock>(Resource.Id.clock);
            _clock.Format24Hour = DateTimeFormat;

            /* Set lock screen button */
            _lockScreenButton = FindViewById<ImageButton>(Resource.Id.lock_screen_button);
            _lockScreenButton.Click += OnLockScreenButtonClicked;
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
    }
}