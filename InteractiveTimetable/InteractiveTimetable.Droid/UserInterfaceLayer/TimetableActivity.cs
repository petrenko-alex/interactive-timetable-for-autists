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
            Console.WriteLine("Management panel");
        }

        private void OnLockScreenButtonClicked(object sender, EventArgs e)
        {
            _mainLayout.Focusable = false;
            Console.WriteLine("Lock screen");
        }
    }
}