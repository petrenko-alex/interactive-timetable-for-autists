using Android.App;
using Android.OS;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "Management", MainLauncher = true)]
    public class ManagementActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

			SetContentView (Resource.Layout.management);

            var userListFragment = UserListFragment.NewInstance();

            var fragmentManager = FragmentManager.BeginTransaction();
            fragmentManager.Add(Resource.Id.user_list, userListFragment);
            fragmentManager.SetTransition(FragmentTransit.FragmentFade);
            fragmentManager.Commit();
        }
    }
}