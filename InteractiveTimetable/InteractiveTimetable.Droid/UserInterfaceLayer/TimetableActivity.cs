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
        private LockableScrollView _timetableTapeScroll;
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

            //_mainLayout.ViewTreeObserver.AddOnGlobalLayoutListener(this);
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
                    fragment.View.Click -= OnLockedScreenClicked;
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
                    fragment.View.Click += OnLockedScreenClicked;
                    fragment.LockFragment();
                }

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
            var widthPx = _timetableTapeLayout.Width;
            var heightPx = _timetableTapeLayout.Height;

            Console.WriteLine($"Layout Width:{widthPx} px");
            Console.WriteLine($"Layout Height:{heightPx} px");

            /* _timetableTapeLayout size in dp */
            var displayMetrics = Resources.DisplayMetrics;
            var dpHeight = heightPx / displayMetrics.Density;
            var dpWidth = widthPx / displayMetrics.Density;

            Console.WriteLine($"Layout Width:{dpWidth} dp");
            Console.WriteLine($"Layout Height:{dpHeight} dp");

            _mainLayout.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);
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
        private void AddTimetableTapeFragment(int userId, IList<Card> scheduleCards)
        {
            /* Create tape fragment */
            var tapeFragment = TimetableTapeFragment.NewInstance(userId, scheduleCards);
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
                var userSchedules = user.Schedules;
                var currentSchedule =
                    userSchedules.Any()
                    ? userSchedules.OrderByDescending(x => x.CreateTime).FirstOrDefault()
                    : null;

                if (currentSchedule != null)
                {
                    var scheduleCards = InteractiveTimetable.Current.ScheduleManager.
                                                             GetScheduleCards(currentSchedule.Id).
                                                             ToList();

                    AddTimetableTapeFragment(user.Id, scheduleCards);
                }
            }
        }

        private void AddFragment(int viewToAdd, Fragment fragmentToAdd, string fragmentTag)
        {
            var transaction = FragmentManager.BeginTransaction();
            transaction.Add(viewToAdd, fragmentToAdd, fragmentTag);
            transaction.SetTransition(FragmentTransit.FragmentFade);
            transaction.Commit();
        }
        #endregion
    }
}