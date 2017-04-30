using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "Management panel")]
    public class ManagementActivity : ActionBarActivity
    {
        #region Constants
        private static readonly string UserIdKey = "current_user_id";
        #endregion

        #region Fragments
        private UserListFragment _userListFragment;
        private UserDetailsFragment _userDetailsFragment;
        private UserDetailsEditFragment _userDetailsEditFragment;
        private InfoFragment _userInfoFragment;
        private InfoFragment _tripInfoFragment;
        private HospitalTripListFragment _tripListFragment;
        private TripDetailsFragment _tripDetailsFragment;
        private TripDetailsEditFragment _tripDetailsEditFragment;
        #endregion

        #region Internal Variables
        private int _currentUserId;
        private int _currentTripId;
        private bool _isWideScreenDevice;
        #endregion

        #region Methods

        #region Event Handlers
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.management);

            /* Set tool bar */
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Window.AddFlags(WindowManagerFlags.Fullscreen);

            /* Hide keyboard */
            Window.SetSoftInputMode(SoftInput.StateAlwaysHidden | SoftInput.AdjustPan);

            /* Determining wide screen device */
            var layout = FindViewById<LinearLayout>(Resource.Id.main_landscape);
            _isWideScreenDevice = layout != null && layout.Visibility == ViewStates.Visible;

            /* Setting fragments */
            AddUserListFragment();
            AddUserDetailsFragment();
            ShowTrips(_currentUserId);
        }

        protected override void OnDestroy()
        {
            _userListFragment.AddUserButtonClicked -= OnAddUserButtonClicked;
            _userListFragment.ListItemClicked -= OnUserListItemClicked;
            _userListFragment.NoMoreUsersInList -= OnNoMoreUsersInList;
            _userDetailsFragment.EditButtonClicked -= OnEditUserButtonClicked;

            base.OnDestroy();
        }

        public void OnUserListItemClicked(int userId)
        {
            if (_isWideScreenDevice)
            {
                DestroyFragment(_userDetailsFragment);

                _userDetailsFragment = UserDetailsFragment.NewInstance(userId);
                _userDetailsFragment.EditButtonClicked += OnEditUserButtonClicked;

                ReplaceFragment(
                    Resource.Id.user_details,
                    _userDetailsFragment,
                    UserDetailsFragment.FragmentTag
                );

                /* Showing user trips */
                ShowTrips(userId);
            }
            else
            {
                var intent = new Intent();
                intent.SetClass(this, typeof(UserDetailsActivivty));
                intent.PutExtra(UserIdKey, userId);
                StartActivity(intent);
            }

            _currentUserId = userId;
        }

        private void OnTripListItemClicked(int tripId)
        {
            /* Determine if possible to add fragment */
            if (!_isWideScreenDevice)
            {
                return;
            }

            /* Destroy previous fragment */
            DestroyFragment(_tripDetailsFragment);

            /* Create and add new fragment */
            _tripDetailsFragment = TripDetailsFragment.NewInstance(tripId);
            _tripDetailsFragment.EditButtonClicked += OnEditTripButtonClicked;

            ReplaceFragment(
                Resource.Id.trip_detailed_info,
                _tripDetailsFragment,
                TripDetailsFragment.FragmentTag
            );

            _currentTripId = tripId;
        }

        public void OnEditUserButtonClicked(int userId)
        {
            DestroyFragment(_userDetailsEditFragment);

            _userDetailsEditFragment = UserDetailsEditFragment.NewInstance(userId);
            _userDetailsEditFragment.UserEdited += OnUserEdited;

            ReplaceFragment(
                Resource.Id.user_details,
                _userDetailsEditFragment,
                UserDetailsEditFragment.FragmentTag,
                true
            );
        }

        private void OnEditTripButtonClicked(int tripId)
        {
            /* Destroy previous fragment */
            DestroyFragment(_tripDetailsEditFragment);

            /* Create and add new fragment */
            _tripDetailsEditFragment = TripDetailsEditFragment.NewInstance(tripId, _currentUserId);
            _tripDetailsEditFragment.TripEdited += OnTripEdited;

            ReplaceFragment(
                Resource.Id.trip_detailed_info,
                _tripDetailsEditFragment,
                TripDetailsEditFragment.FragmentTag,
                true
            );
        }

        public void OnAddUserButtonClicked()
        {
            /* Destroy previous fragment */
            DestroyFragment(_userDetailsEditFragment);

            AdjustTripLayoutVisibility(true);

            DetachFragment(_userInfoFragment);

            /* Create and add new fragment */
            _userDetailsEditFragment = UserDetailsEditFragment.NewInstance(0);
            _userDetailsEditFragment.NewUserAdded += OnNewUserAdded;
            _userDetailsEditFragment.EditCanceled += OnEditUserCanceled;

            ReplaceFragment(
                Resource.Id.user_details,
                _userDetailsEditFragment,
                UserDetailsEditFragment.FragmentTag,
                true
            );
        }

        private void OnEditUserCanceled()
        {
            if (_userListFragment != null &&
                !_userListFragment.IsListEmpty())
            {
                AdjustTripLayoutVisibility(false);
            }
            else if (_userListFragment != null &&
                     _userListFragment.IsListEmpty())
            {
                AttachFragment(_userInfoFragment);
            }
        }

        private void OnAddTripButtonClicked()
        {
            /* Destroy previous fragment */
            DestroyFragment(_tripDetailsEditFragment);

            /* Create and add new fragment */
            _tripDetailsEditFragment = TripDetailsEditFragment.NewInstance(0, _currentUserId);
            _tripDetailsEditFragment.NewTripAdded += OnNewTripAdded;

            ReplaceFragment(
                Resource.Id.trip_detailed_info,
                _tripDetailsEditFragment,
                TripDetailsEditFragment.FragmentTag,
                true
            );
        }

        private void OnNewTripAdded(int tripId)
        {
            _tripListFragment.DataSetChanged();

            /* If info fragment is present, destroy it */
            DestroyFragment(_tripInfoFragment);

            OnTripListItemClicked(tripId);
        }

        public void OnNewUserAdded(int userId)
        {
            _userListFragment.AddUser(userId);

            /* If info fragment is present, detach it */
            DestroyFragment(_userInfoFragment);

            OnUserListItemClicked(userId);
            AdjustUserCardLayoutVisibility(false);
        }

        public void OnUserEdited(int userId)
        {
            _userListFragment.DataSetChanged();
        }

        private void OnTripEdited(int tripId)
        {
            _tripListFragment.DataSetChanged();
        }

        public void OnNoMoreUsersInList()
        {
            /* Destroy connected fragments */
            DestroyFragment(_userDetailsFragment);
            DestroyFragment(_tripListFragment);
            DestroyFragment(_tripDetailsFragment);
            AdjustTripLayoutVisibility(true);
            AdjustUserCardLayoutVisibility(true);

            /* Add info fragment */
            DestroyFragment(_userInfoFragment);
            _userInfoFragment = InfoFragment.NewInstance(GetString(Resource.String.detailed_user_info));

            ReplaceFragment(
                Resource.Id.user_details_and_trips,
                _userInfoFragment,
                InfoFragment.FragmentTag
            );
        }

        private void OnNoMoreTripsInList()
        {
            /* Destroy connected fragments */
            DestroyFragment(_tripDetailsFragment);
            
            /* Add info fragment */
            DestroyFragment(_tripInfoFragment);
            _tripInfoFragment = InfoFragment.NewInstance(GetString(Resource.String.detailed_trip_info));

            ReplaceFragment(
                Resource.Id.trip_detailed_info,
                _tripInfoFragment,
                InfoFragment.FragmentTag
            );
        }

        private void OnTimetableButtonClicked()
        {
            var intent = new Intent(this, typeof(TimetableActivity));
            SetResult(Result.Ok, intent);
            Finish();
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();

            if (_userInfoFragment != null)
            {
                AttachFragment(_userInfoFragment);
            }
            else if(_userDetailsFragment != null)
            {
                AdjustTripLayoutVisibility(false);
            }
        }

        #endregion

        #region Other Methods
        private void AddUserListFragment()
        {
            /* Trying to find fragment */
            _userListFragment = FragmentManager.FindFragmentByTag(UserListFragment.FragmentTag)
                as UserListFragment;

            /* Creting a new one if not exist */
            if (_userListFragment == null)
            {
                _userListFragment = UserListFragment.NewInstance();
                _userListFragment.ListItemClicked += OnUserListItemClicked;
                _userListFragment.AddUserButtonClicked += OnAddUserButtonClicked;
                _userListFragment.NoMoreUsersInList += OnNoMoreUsersInList;
                _userListFragment.TimetableButtonClicked += OnTimetableButtonClicked;

                ReplaceFragment(
                    Resource.Id.user_list,
                    _userListFragment,
                    UserListFragment.FragmentTag
                );
            }
        }

        private void ShowTrips(int userId)
        {
            if (_isWideScreenDevice && _userDetailsFragment != null)
            {
                /* Destroy previous trip list */
                DestroyFragment(_tripListFragment);

                AdjustTripLayoutVisibility(false);
                SetTripsLabel(userId);

                /* Create new fragment with trip list */
                _tripListFragment = HospitalTripListFragment.NewInstance(userId);
                _tripListFragment.ListItemClicked += OnTripListItemClicked;
                _tripListFragment.AddTripButtonClicked += OnAddTripButtonClicked;
                _tripListFragment.NoMoreTripsInList += OnNoMoreTripsInList;

                ReplaceFragment(
                    Resource.Id.trip_list,
                    _tripListFragment,
                    HospitalTripListFragment.FragmentTag
                );

                /* Add trip detailed info */
                var tripId = _tripListFragment.GetFirstTripId();
                AddTripDetailsFragment(tripId);

            }
            else
            {
                AdjustTripLayoutVisibility(true);
            }
        }

        private void AddUserDetailsFragment()
        {
            /* Determine if list is empty or not */
            bool isListEmpty = false;
            if(_userListFragment != null)
            {
                isListEmpty = _userListFragment.IsListEmpty();
            }

            if (_isWideScreenDevice && !isListEmpty)
            {
                _userDetailsFragment = FragmentManager.FindFragmentByTag(UserDetailsFragment.FragmentTag)
                        as UserDetailsFragment;

                if (_userDetailsFragment == null)
                {
                    _currentUserId = _userListFragment.GetFirstUserId();
                    _userDetailsFragment = UserDetailsFragment.NewInstance(_currentUserId);
                    _userDetailsFragment.EditButtonClicked += OnEditUserButtonClicked;

                    ReplaceFragment(
                        Resource.Id.user_details,
                        _userDetailsFragment,
                        UserDetailsFragment.FragmentTag
                    );
                }
            }
        }

        private void AddTripDetailsFragment(int tripId)
        {
            /* Determine if possible to add fragment */
            bool isListEmpty = false;
            if (!_isWideScreenDevice ||
                _tripListFragment == null || 
                _userDetailsFragment == null)
            {
                return;
            }

            /* Add fragment if user has trips */
            isListEmpty = _tripListFragment.IsListEmpty();
            if (!isListEmpty)
            {
                /* Destroy previous fragment */
                DestroyFragment(_tripDetailsFragment);

                /* Create and add new fragment */
                _tripDetailsFragment = TripDetailsFragment.NewInstance(tripId);
                _tripDetailsFragment.EditButtonClicked += OnEditTripButtonClicked;

                ReplaceFragment(
                    Resource.Id.trip_detailed_info,
                    _tripDetailsFragment,
                    TripDetailsFragment.FragmentTag
                );

                _currentTripId = tripId;
            }
        }
        
        private void DetachFragment(Fragment fragmentToDetach)
        {
            if (fragmentToDetach != null)
            {
                var transaction = FragmentManager.BeginTransaction();
                transaction.Detach(fragmentToDetach);
                transaction.Commit();
            }
        }

        private void AttachFragment(Fragment fragmentToAttach)
        {
            if (fragmentToAttach != null)
            {
                var transaction = FragmentManager.BeginTransaction();
                transaction.Attach(fragmentToAttach);
                transaction.Commit();
            }
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

        private void ReplaceFragment(
            int viewToAdd, 
            Fragment fragmentToAdd, 
            string fragmentTag, 
            bool needToAddToBackStack = false)
        {
            var transaction = FragmentManager.BeginTransaction();
            transaction.Replace(viewToAdd, fragmentToAdd, fragmentTag);
            transaction.SetTransition(FragmentTransit.FragmentFade);

            if (FragmentManager.BackStackEntryCount > 0)
            {
                FragmentManager.PopBackStackImmediate();
            }

            if (needToAddToBackStack)
            {
                transaction.AddToBackStack(fragmentTag);
            }
            transaction.Commit();
        }

        private void SetTripsLabel(int userId)
        {
            var tripLabel = FindViewById<TextView>(Resource.Id.trips_label);
            tripLabel.Text = GetString(Resource.String.trips);
        }

        private void AdjustTripLayoutVisibility(bool isHidden)
        {
            var tripLabel = FindViewById<TextView>(Resource.Id.trips_label);
            var divider = FindViewById<View>(Resource.Id.trips_divider);
            var frame = FindViewById<FrameLayout>(Resource.Id.user_trips);

            if (isHidden)
            {
                tripLabel.Visibility = ViewStates.Gone;
                divider.Visibility = ViewStates.Gone;
                frame.Visibility = ViewStates.Gone;
            }
            else
            {
                tripLabel.Visibility = ViewStates.Visible;
                divider.Visibility = ViewStates.Visible;
                frame.Visibility = ViewStates.Visible;
            }
        }

        private void AdjustUserCardLayoutVisibility(bool isHidden)
        {
            var userCardLabel = FindViewById<View>(Resource.Id.user_card_label);
            var userCardDivider = FindViewById<View>(Resource.Id.user_card_divider);

            if (isHidden)
            {
                userCardLabel.Visibility = ViewStates.Gone;
                userCardDivider.Visibility = ViewStates.Gone;
            }
            else
            {
                userCardLabel.Visibility = ViewStates.Visible;
                userCardDivider.Visibility = ViewStates.Visible;
            }
        }
        #endregion

        #endregion
    }
}