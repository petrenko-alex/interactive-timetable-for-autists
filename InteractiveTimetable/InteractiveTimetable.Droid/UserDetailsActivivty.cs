using Android.App;
using Android.Content;
using Android.OS;
using InteractiveTimetable.Droid.UserInterfaceLayer;

namespace InteractiveTimetable.Droid
{
    [Activity(Label = "UserDetailsActivivty")]
    public class UserDetailsActivivty : Activity
    {
        private static readonly string UserIdKey = "current_user_id";
        public static readonly string FragmentTag = "user_details_fragment";

        private UserDetailsFragment _userDetailsFragment;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //SetContentView(Resource.Layout.user_details);

            var userId = Intent.Extras.GetInt(UserIdKey, 0);
            
            /* Trying to find fragment */
            _userDetailsFragment = FragmentManager.FindFragmentByTag(UserDetailsFragment.FragmentTag)
                as UserDetailsFragment;

            /* Creting a new one if not exist */
            if (_userDetailsFragment == null)
            {
                _userDetailsFragment = UserDetailsFragment.NewInstance(userId);

                var fragmentManager = FragmentManager.BeginTransaction();
                fragmentManager.Replace(
                        Android.Resource.Id.Content,
                        _userDetailsFragment,
                        UserListFragment.FragmentTag
                    );

                fragmentManager.SetTransition(FragmentTransit.FragmentFade);
                fragmentManager.Commit();
            }
        }
    }
}