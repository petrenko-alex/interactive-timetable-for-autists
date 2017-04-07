using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "Management", MainLauncher = true)]
    public class ManagementActivity : Activity
    {
        #region Constants
        private static readonly string UserIdKey = "current_user_id";
        #endregion

        #region Fragments
        private UserListFragment _userListFragment;
        private UserDetailsFragment _userDetailsFragment;
        private UserDetailsEditFragment _userDetailsEditFragment;
        private InfoFragment _infoFragment;
        private HospitalTripListFragment _tripListFragment;
        private TripDetailsFragment _tripDetailsFragment;
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

            /* Hide keyboard */
            Window.SetSoftInputMode(SoftInput.StateAlwaysHidden);

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
            _userDetailsEditFragment.NewUserAdded -= OnNewUserAdded;
            _userDetailsEditFragment.UserEdited -= OnUserEdited;

            base.OnDestroy();
        }

        public void OnUserListItemClicked(int userId)
        {
            if (_isWideScreenDevice)
            {
                /* Showing detailed info about user */
                _userDetailsFragment = FragmentManager.FindFragmentByTag(UserDetailsFragment.FragmentTag)
                        as UserDetailsFragment;

                if (_userDetailsFragment == null || _currentUserId != userId)
                {
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

            /* If need to show another trip info */
            if (_currentTripId != tripId)
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
            }

            _currentTripId = tripId;
        }

        public void OnEditUserButtonClicked(int userId)
        {
            _userDetailsEditFragment = FragmentManager.FindFragmentByTag(UserDetailsEditFragment.FragmentTag)
                        as UserDetailsEditFragment;

            if (_userDetailsEditFragment == null)
            {
                _userDetailsEditFragment = UserDetailsEditFragment.NewInstance(userId);
                _userDetailsEditFragment.UserEdited += OnUserEdited;

                var fragmentManager = FragmentManager.BeginTransaction();
                fragmentManager.Replace(
                        Resource.Id.user_details,
                        _userDetailsEditFragment,
                        UserDetailsEditFragment.FragmentTag
                    );

                fragmentManager.SetTransition(FragmentTransit.FragmentFade);
                fragmentManager.AddToBackStack(UserDetailsEditFragment.FragmentTag);
                fragmentManager.Commit();
            }
        }

        public void OnAddUserButtonClicked()
        {
            _userDetailsEditFragment = FragmentManager.FindFragmentByTag(UserDetailsEditFragment.FragmentTag)
                        as UserDetailsEditFragment;

            if (_userDetailsEditFragment == null)
            {
                _userDetailsEditFragment = UserDetailsEditFragment.NewInstance(0);
                _userDetailsEditFragment.NewUserAdded += OnNewUserAdded;

                /* In info fragment is present, detach it */
                DetachFragment(_infoFragment);

                var fragmentManager = FragmentManager.BeginTransaction();
                fragmentManager.Replace(
                        Resource.Id.user_details,
                        _userDetailsEditFragment,
                        UserDetailsEditFragment.FragmentTag
                    );

                fragmentManager.SetTransition(FragmentTransit.FragmentFade);
                fragmentManager.AddToBackStack(UserDetailsEditFragment.FragmentTag);
                fragmentManager.Commit();
            }
        }

        public void OnNewUserAdded(int userId)
        {
            _userListFragment.AddUser(userId);

            /* If info fragment is present, detach it */
            DetachFragment(_infoFragment);

            OnUserListItemClicked(userId);
        }

        public void OnUserEdited(int userId)
        {
            _userListFragment.DataSetChanged();
        }

        public void OnNoMoreUsersInList()
        {
            /* Destroy connected fragments */
            DestroyFragment(_userDetailsFragment);
            DestroyFragment(_tripListFragment);
            DestroyFragment(_tripDetailsFragment);
            AdjustTripLayoutVisibility(true);

            /* Add info fragment */
            DestroyFragment(_infoFragment);
            _infoFragment = InfoFragment.NewInstance(GetString(Resource.String.detailed_user_info));

            ReplaceFragment(
                Resource.Id.user_details_and_trips,
                _infoFragment,
                InfoFragment.FragmentTag
            );
        }

        private void OnNoMoreTripsInList()
        {
            /* Destroy connected fragments */
            DestroyFragment(_tripDetailsFragment);
            
            /* Add info fragment */
            DestroyFragment(_infoFragment);
            _infoFragment = InfoFragment.NewInstance(GetString(Resource.String.detailed_trip_info));

            ReplaceFragment(
                Resource.Id.trip_detailed_info,
                _infoFragment,
                InfoFragment.FragmentTag
            );
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

        private void OnAddTripButtonClicked()
        {
            throw new System.NotImplementedException();
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

        private void OnEditTripButtonClicked(int tripId)
        {
            throw new System.NotImplementedException();
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

        private void ReplaceFragment(int viewToAdd, Fragment fragmentToAdd, string fragmentTag)
        {
            var transaction = FragmentManager.BeginTransaction();
            transaction.Replace(viewToAdd, fragmentToAdd, fragmentTag);
            transaction.SetTransition(FragmentTransit.FragmentFade);
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
        #endregion

        #endregion
    }
}