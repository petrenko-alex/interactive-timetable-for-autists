using Android.App;
using Android.OS;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "Management", MainLauncher = true)]
    public class ManagementActivity : Activity
    {
        public static readonly string UserListFragmentTag = "user_list_fragment";

        private UserListFragment _userListFragment;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

			SetContentView (Resource.Layout.management);

            AddUserListFragment();
        }

        private void AddUserListFragment()
        {
            /* Trying to find fragment */
            _userListFragment = FragmentManager.FindFragmentByTag(UserListFragmentTag) 
                as UserListFragment;

            /* Creting a new one if not exist */
            if (_userListFragment == null)
            {
                _userListFragment = UserListFragment.NewInstance();

                var fragmentManager = FragmentManager.BeginTransaction();
                fragmentManager.Replace(Resource.Id.user_list, _userListFragment, UserListFragmentTag);
                fragmentManager.SetTransition(FragmentTransit.FragmentFade);
                fragmentManager.Commit();
            }
        }
    }
}