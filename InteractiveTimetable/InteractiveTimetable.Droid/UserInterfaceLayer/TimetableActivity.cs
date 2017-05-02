using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.Droid.ApplicationLayer;
using System.Collections.Generic;
using Android.Support.V7.App;
using InteractiveTimetable.BusinessLayer.Models;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "Interactive Timetable", MainLauncher = false)]
    public class TimetableActivity : ActionBarActivity
    {
        #region Constants
        private static readonly string DateTimeFormat = "d MMMM yyyy, EEEE   k:mm";
        private static readonly int CreateTimetableRequest = 0;
        private static readonly int ManageUsersRequest = 1;
        #endregion

        #region Widgets
        private RelativeLayout _mainLayout;
        private LinearLayout _timetableTapeLayout;
        private LinearLayout _timetableInfoLayout;
        private ImageButton _homeScreenButton;
        private ImageButton _lockScreenButton;
        private Button _goAndAddButton;
        private LockableScrollView _timetableTapeScroll;
        private Toast _toastMessage;
        #endregion

        #region Internal Variables
        private DateTime _chosenDate;
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
            OverridePendingTransition(
                Resource.Animation.enter_from_right,
                Resource.Animation.exit_to_left
            );

            /* Set class variables */
            _chosenDate = DateTime.Today;

            /* Set tool bar */
            var toolbar = FindViewById<Toolbar>(Resource.Id.t_toolbar);
            SetSupportActionBar(toolbar);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            AdjustToolbarForActivity();

            /* Get layouts */
            _mainLayout = FindViewById<RelativeLayout>(Resource.Id.timetable_main_layout);
            _timetableTapeLayout = FindViewById<LinearLayout>(Resource.Id.timetable_tape_layout);
            _timetableTapeScroll = FindViewById<LockableScrollView>(Resource.Id.timetable_tape_scroll);
            _timetableInfoLayout = FindViewById<LinearLayout>(Resource.Id.timetable_info_layout);

            /* Set go and add button */
            _goAndAddButton = FindViewById<Button>(Resource.Id.go_and_add_button);
            _goAndAddButton.Click += OnManagementPanelButtonClicked;

            /* Add timetable tapes */
            AddTimetableTapeFragments();

            /* Mark yesterday schedules as finished */
            FinishYesterdaySchedules();
        }

        private void AdjustToolbarForActivity()
        {
            /* Set toolbar layout */
            var toolbar = FindViewById<Toolbar>(Resource.Id.t_toolbar);
            var toolbarContent = FindViewById<LinearLayout>(Resource.Id.toolbar_content);
            var layout = LayoutInflater.Inflate(Resource.Layout.timetable_toolbar, toolbar, false);
            toolbarContent.AddView(layout);

            /* Set toolbar controls */
            var title = toolbar.FindViewById<TextView>(Resource.Id.toolbar_title);
            title.Text = GetString(Resource.String.timetable_for_the_day);

            var clock = toolbar.FindViewById<TextClock>(Resource.Id.toolbar_clock);
            clock.Format24Hour = InteractiveTimetable.DateTimeFormat;

            _lockScreenButton = toolbar.FindViewById<ImageButton>(Resource.Id.toolbar_lock);
            _lockScreenButton.Click += OnLockScreenButtonClicked;

            _homeScreenButton = toolbar.FindViewById<ImageButton>(Resource.Id.toolbar_home);
            _homeScreenButton.Click += OnHomeScreenButtonClicked;

            var chosenDate = toolbar.FindViewById<TextView>(Resource.Id.toolbar_chosen_date);
            chosenDate.Text = GetString(Resource.String.chosen_date) + ": " + _chosenDate.ToString("D");
        }

        private void OnHomeScreenButtonClicked(object sender, EventArgs e)
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

        private void FinishYesterdaySchedules()
        {
            var yesterday = DateTime.Today.AddDays(-1);

            /* For all users */
            var users = InteractiveTimetable.Current.UserManager.GetUsers();
            foreach (var user in users)
            {
                /* For all user schedules that was created yesterday and was not completed */
                var schedules = InteractiveTimetable.Current.ScheduleManager.
                                                     GetSchedules(user.Id).
                                                     Where(x => x.CreateTime.Date.Equals(yesterday) 
                                                     && !x.IsCompleted);
                /* Finish schedule */
                foreach (var schedule in schedules)
                {
                    InteractiveTimetable.Current.ScheduleManager.FinishSchedule(schedule.Id);
                }
            }
        }

        private void OnManagementPanelButtonClicked(object sender, EventArgs e)
        {
            /* Start management activity */
            var intent = new Intent(this, typeof(ManagementActivity));
            StartActivityForResult(intent, ManageUsersRequest);
        }

        private void OnLockScreenButtonClicked(object sender, EventArgs e)
        {
            /* If locked, need to unlock */
            if (_isLocked)
            {
                /* Reset handler for locked screen */
                _homeScreenButton.Click -= OnLockedScreenClicked;
                _mainLayout.Click -= OnLockedScreenClicked;
                _timetableTapeScroll.IsScrollEnabled = true;
                foreach (var fragment in _tapeFragments)
                {
                    fragment.UnlockFragment();
                }

                /* Set function handlers*/
                _homeScreenButton.Click += OnHomeScreenButtonClicked;

                /* Set button image */
                _lockScreenButton.SetImageResource(Resource.Drawable.unlocked_icon);

                _isLocked = false;
            }
            /* If unlocked, need to lock */
            else
            {
                /* Reset function handlers */
                _homeScreenButton.Click -= OnHomeScreenButtonClicked;

                /* Set handler for lock screen */
                _homeScreenButton.Click += OnLockedScreenClicked;
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

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok && requestCode == ManageUsersRequest)
            {
                var newUsers = InteractiveTimetable.Current.UserManager.
                                                    GetUsersForCurrentTimetable().
                                                    ToList();

                var currentUserIds = _tapeFragments.Select(x => x.UserId).ToList();
                var newUserIds = newUsers.Select(x => x.Id).ToList();

                /* Get users to delete */
                var usersToDelete = currentUserIds.Except(newUserIds).ToList();

                /* Delete tapes */
                foreach (var userId in usersToDelete)
                {
                    var tapeToDelete = _tapeFragments.First(x => x.UserId == userId);
                    DestroyFragment(tapeToDelete);
                    _tapeFragments.Remove(tapeToDelete);
                }

                /* In case there is no more tapes */
                if (_tapeFragments.Count == 0)
                {
                    ShowNoUsersInfo();
                }

                /* Get users to add */
                var usersToAdd = newUserIds.Except(currentUserIds).ToList();

                /* Add tapes */
                foreach (var userId in usersToAdd)
                {
                    AddTimetableTapeFragment(userId, new List<ScheduleItem>());
                }

                /* In case there were no tapes before */
                if (_tapeFragments.Count > 0 && 
                    _timetableInfoLayout.Visibility == ViewStates.Visible)
                {
                    ShowTapes();
                }

                /* Refresh user names and photos */
                foreach (var tapeFragment in _tapeFragments)
                {
                    tapeFragment.RefreshUserInfo();
                }
            }
            else if (resultCode == Result.Ok && requestCode == CreateTimetableRequest)
            {
                var id = 0;
                var cards = data.GetIntArrayExtra("cards").ToList();
                int tapeNumber = data.GetIntExtra("tape_number", 0);

                var tapeFragment = _tapeFragments[tapeNumber];

                /* If need to edit existing schedule */
                if (tapeFragment.CurrentSchedule != null &&
                    tapeFragment.CurrentSchedule.Id > 0 &&
                    !tapeFragment.CurrentSchedule.IsCompleted)
                {
                    /* Update schedule in data base */
                    id = InteractiveTimetable.Current.ScheduleManager.UpdateSchedule(
                        tapeFragment.CurrentSchedule.Id,
                        cards
                    );
                }
                /* If need to create new schedule */
                else
                {
                    /* Create schedule in database */
                    id = InteractiveTimetable.Current.ScheduleManager.SaveSchedule(
                        tapeFragment.UserId,
                        cards
                    );
                }

                /* Set schedule in tape fragment */
                var schedule = InteractiveTimetable.Current.ScheduleManager.GetSchedule(id);
                _tapeFragments[tapeNumber].SetSchedule(schedule.ScheduleItems);
            }
        }

        private void OnEditTimetableTapeButtonClicked(
            int userId,
            int tapeNumber, 
            IList<Card>  cards)
        {
            if (_isLocked)
            {
                OnLockedScreenClicked(this, null);
                return;
            }

            /* Create and start activity */
            var intent = new Intent(this, typeof(CreateTimetableActivity));
            intent.PutExtra("user_id", userId);
            intent.PutExtra("date", DateTime.Today.ToString("dd.MM.yyyy"));
            intent.PutExtra("tape_number", tapeNumber);
            intent.PutExtra("cards", cards.Select(card => ParcelableCard.FromCard(card)).ToArray());
            StartActivityForResult(intent, CreateTimetableRequest);
        }

        private void OnTriedToScrollWhenLocked()
        {
            OnLockedScreenClicked(this, null);
        }

        #region Other Methods
        private void AddTimetableTapeFragment(int userId, IList<ScheduleItem> scheduleItems)
        {
            /* Create tape fragment */
            var tapeFragment = TimetableTapeFragment.NewInstance(
                userId,
                scheduleItems,
                _tapeFragments.Count
            );
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

        private void DestroyFragment(Fragment fragmentToDestroy)
        {
            if (fragmentToDestroy != null)
            {
                var transaction = FragmentManager.BeginTransaction();
                transaction.Remove(fragmentToDestroy);
                transaction.Commit();
            }
        }

        private void ShowNoUsersInfo()
        {
            _timetableTapeScroll.Visibility = ViewStates.Gone;
            _lockScreenButton.Visibility = ViewStates.Gone;           

            _timetableInfoLayout.Visibility = ViewStates.Visible;
        }

        private void ShowTapes()
        {
            _timetableTapeScroll.Visibility = ViewStates.Visible;
            _lockScreenButton.Visibility = ViewStates.Visible;

            _timetableInfoLayout.Visibility = ViewStates.Gone;
        }
        #endregion
    }
}