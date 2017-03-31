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
        private static readonly string UserIdKey = "current_user_id";

        private UserListFragment _userListFragment;
        private UserDetailsFragment _userDetailsFragment;
        private UserDetailsEditFragment _userDetailsEditFragment;

        private int _currentUserId;

        private bool _isWideScreenDevice;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

			SetContentView (Resource.Layout.management);

            /* Hide keyboard */
            Window.SetSoftInputMode(SoftInput.StateAlwaysHidden);

            /* Determining wide screen device */
            var layout = FindViewById<LinearLayout>(Resource.Id.main_landscape);
            _isWideScreenDevice = layout != null && layout.Visibility == ViewStates.Visible;

            AddUserListFragment();
            AddUserDetailsFragment();
        }

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

        private void AddUserDetailsFragment()
        {
            if (_isWideScreenDevice)
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

        protected override void OnDestroy()
        {
            _userListFragment.AddUserButtonClicked -= OnAddUserButtonClicked;
            _userListFragment.ListItemClicked -= OnUserListItemClicked;
            _userDetailsFragment.EditButtonClicked -= OnEditUserButtonClicked;

            base.OnDestroy();
        }

        public void OnAddUserButtonClicked()
        {
            var editUserDetails = FragmentManager.FindFragmentByTag(UserDetailsEditFragment.FragmentTag)
                        as UserDetailsEditFragment;

            if (editUserDetails == null)
            {
                var editUserDetailsFragment = UserDetailsEditFragment.NewInstance(0);

                var fragmentManager = FragmentManager.BeginTransaction();
                fragmentManager.Replace(
                        Resource.Id.user_details,
                        editUserDetailsFragment,
                        UserDetailsEditFragment.FragmentTag
                    );

                fragmentManager.SetTransition(FragmentTransit.FragmentFade);
                fragmentManager.AddToBackStack(UserDetailsEditFragment.FragmentTag);
                fragmentManager.Commit();
            }
        }

        public void OnUserListItemClicked(int userId)
        {
            if (_isWideScreenDevice)
            {
                _userDetailsFragment = FragmentManager.FindFragmentByTag(UserDetailsFragment.FragmentTag)
                        as UserDetailsFragment;

                if (_userDetailsFragment == null  || _currentUserId != userId)
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
    }
}