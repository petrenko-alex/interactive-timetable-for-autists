using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.Droid.ApplicationLayer;
using System.Collections.Generic;
using InteractiveTimetable.BusinessLayer.Models;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "Interactive Timetable", MainLauncher = true/*, Theme = "@android:style/Theme.Holo.Light"*/)]
    public class TimetableActivity : Activity
    {
        #region Constants
        private static readonly string DateTimeFormat = "d MMMM yyyy, EEEE   k:mm";
        #endregion

        #region Widgets
        private LinearLayout _mainLayout;
        private LinearLayout _timetableTapeLayout;
        private LinearLayout _timetableInfoLayout;
        private TextClock _clock;
        private ImageButton _managementPanelButton;
        private ImageButton _lockScreenButton;
        private Button _goAndAddButton;
        private LockableScrollView _timetableTapeScroll;
        private Toast _toastMessage;
        #endregion

        #region Fragments
        private IList<TimetableTapeFragment> _tapeFragments;
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
            _timetableTapeScroll = FindViewById<LockableScrollView>(Resource.Id.timetable_tape_scroll);
            _timetableInfoLayout = FindViewById<LinearLayout>(Resource.Id.timetable_info_layout);
           
            /* Set go and add button */
            _goAndAddButton = FindViewById<Button>(Resource.Id.go_and_add_button);
            _goAndAddButton.Click += OnManagementPanelButtonClicked;

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
            AddTimetableTapeFragments();
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
                _timetableTapeScroll.IsScrollEnabled = true;
                foreach (var fragment in _tapeFragments)
                {
                    fragment.UnlockFragment();
                }

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
                _timetableTapeScroll.IsScrollEnabled = false;
                foreach (var fragment in _tapeFragments)
                {
                    fragment.LockFragment();
                }

                /* Set button image */
                _lockScreenButton.SetImageResource(Resource.Drawable.locked_icon);

                _isLocked = true;
            }
        }

        private void OnLockedScreenClicked(object sender, EventArgs e)
        {
            if (_toastMessage == null ||
                _toastMessage.View.WindowVisibility != ViewStates.Visible)
            {
                _toastMessage = ToastHelper.GetInfoToast(this, GetString(Resource.String.screen_is_locked));
                _toastMessage.Show();
            }
        }

        private void OnEditTimetableTapeButtonClicked(int userId)
        {
            if (_isLocked)
            {
                OnLockedScreenClicked(this, null);
                return;
            }
            Console.WriteLine("Edit timetable tape button clicked");
        }

        private void OnTriedToScrollWhenLocked()
        {
            OnLockedScreenClicked(this, null);
        }

        #region Other Methods
        private void AddTimetableTapeFragment(int userId, IList<ScheduleItem> scheduleItems)
        {
            /* Create tape fragment */
            var tapeFragment = TimetableTapeFragment.NewInstance(userId, scheduleItems);
            tapeFragment.EditTimetableTapeButtonClicked += OnEditTimetableTapeButtonClicked;
            tapeFragment.ClickedWhenLocked += OnLockedScreenClicked;

            /* Add fragment to fragments list */
            _tapeFragments.Add(tapeFragment);

            /* Add fragment to layout */
            AddFragment(
                Resource.Id.timetable_tape_layout,
                tapeFragment,
                TimetableTapeFragment.FragmentTag + "_" + userId
            );
        }

        private void AddTimetableTapeFragments()
        {
            _tapeFragments = new List<TimetableTapeFragment>();
            var currentUsers = InteractiveTimetable.Current.UserManager.
                                                    GetUsersForCurrentTimetable().
                                                    ToList();

            foreach (var user in currentUsers)
            {
                /* Get current user schedule as latest schedule */
                var scheduleItems = new List<ScheduleItem>();
                var userSchedules = user.Schedules;
                var currentSchedule =
                    userSchedules.Any()
                    ? userSchedules.OrderByDescending(x => x.CreateTime).FirstOrDefault()
                    : null;

                /* If user has today timetable */
                if (currentSchedule != null && currentSchedule.CreateTime.Date.Equals(DateTime.Today))
                {
                    scheduleItems = currentSchedule.ScheduleItems.ToList();
                }

                AddTimetableTapeFragment(user.Id, scheduleItems);
            }

            /* Show info message if no users with current trips */
            if (!currentUsers.Any())
            {
                ShowNoUsersInfo();
            }
        }

        private void AddFragment(int viewToAdd, Fragment fragmentToAdd, string fragmentTag)
        {
            var transaction = FragmentManager.BeginTransaction();
            transaction.Add(viewToAdd, fragmentToAdd, fragmentTag);
            transaction.SetTransition(FragmentTransit.FragmentFade);
            transaction.Commit();
        }

        private void ShowNoUsersInfo()
        {
            _timetableTapeScroll.Visibility = ViewStates.Gone;
            _lockScreenButton.Visibility = ViewStates.Gone;
            FindViewById<TextView>(Resource.Id.our_kids_label).Visibility = ViewStates.Gone;
            FindViewById<TextView>(Resource.Id.day_timetable_label).Visibility = ViewStates.Gone;

            _timetableInfoLayout.Visibility = ViewStates.Visible;
        }
        #endregion
    }
}