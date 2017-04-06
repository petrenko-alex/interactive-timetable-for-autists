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
        #endregion

        #region Internal Variables
        private int _currentUserId;
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
            AddTripListFragment(_currentUserId);
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

                    var fragmentManager = FragmentManager.BeginTransaction();
                    fragmentManager.Replace(
                            Resource.Id.user_details,
                            _userDetailsFragment,
                            UserDetailsFragment.FragmentTag
                        );

                    fragmentManager.SetTransition(FragmentTransit.FragmentFade);
                    fragmentManager.Commit();
                }

                /* Showing user trips */
                AddTripListFragment(userId);
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
            DestroyFragment(_userDetailsFragment);
            DestroyFragment(_tripListFragment);
            AdjustTripLayoutVisibility(true);

            _infoFragment = FragmentManager.FindFragmentByTag(InfoFragment.FragmentTag) as InfoFragment;

            if (_infoFragment == null)
            {
                _infoFragment = InfoFragment.NewInstance(GetString(Resource.String.detailed_user_info));

                var fragmentManager = FragmentManager.BeginTransaction();

                fragmentManager.Replace(
                    Resource.Id.user_details_and_trips,
                    _infoFragment,
                    InfoFragment.FragmentTag
                );
                fragmentManager.SetTransition(FragmentTransit.FragmentFade);
                fragmentManager.Commit();
            }
            else
            {
                AttachFragment(_infoFragment);
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

                var fragmentManager = FragmentManager.BeginTransaction();
                fragmentManager.Replace(
                        Resource.Id.user_list,
                        _userListFragment,
                        UserListFragment.FragmentTag
                    );

                fragmentManager.SetTransition(FragmentTransit.FragmentFade);
                fragmentManager.Commit();
            }
        }

        private void AddTripListFragment(int userId)
        {
            if (_isWideScreenDevice && _userDetailsFragment != null)
            {
                /* Destroy previous trip list */
                DestroyFragment(_tripListFragment);

                AdjustTripLayoutVisibility(false);
                SetTripsLabel(userId);

                /* Create new fragment */
                _tripListFragment = HospitalTripListFragment.NewInstance(userId);
                _tripListFragment.ListItemClicked += OnTripListItemClicked;
                _tripListFragment.AddTripButtonClicked += OnAddTripButtonClicked;
                _tripListFragment.NoMoreTripsInList += OnNoMoreTripsInList;

                var fragmentTransaction = FragmentManager.BeginTransaction();
                fragmentTransaction.Replace(
                    Resource.Id.trip_list,
                    _tripListFragment,
                    HospitalTripListFragment.FragmentTag
                );
                fragmentTransaction.SetTransition(FragmentTransit.FragmentFade);
                fragmentTransaction.Commit();
            }
            else
            {
                AdjustTripLayoutVisibility(true);
            }
        }

        private void OnNoMoreTripsInList()
        {
            //throw new System.NotImplementedException();
        }

        private void OnAddTripButtonClicked()
        {
            throw new System.NotImplementedException();
        }

        private void OnTripListItemClicked(int tripId)
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

                    var fragmentManager = FragmentManager.BeginTransaction();
                    fragmentManager.Replace(
                            Resource.Id.user_details,
                            _userDetailsFragment,
                            UserDetailsFragment.FragmentTag
                        );

                    fragmentManager.SetTransition(FragmentTransit.FragmentFade);
                    fragmentManager.Commit();
                }
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