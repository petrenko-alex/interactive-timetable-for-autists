using System.Linq;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.Droid.ApplicationLayer;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    public class UserListFragment : Fragment
    {
        private RecyclerView _recyclerView;
        private RecyclerView.LayoutManager _layoutManager;
        private UserListAdapter _userListAdapter;

        private bool _isWideScreenDevice;

        public int UserId
        {
            get { return Arguments.GetInt("user_id", 0); }
        }
        
        public static UserListFragment NewInstance()
        {
            var userListFragment = new UserListFragment
            {
                Arguments = new Bundle()
            };

            return userListFragment;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            /* Getting views */
            _recyclerView = Activity.FindViewById<RecyclerView>(Resource.Id.user_recycler_view);

            /* Setting up the layout manager */
            _layoutManager = new LinearLayoutManager(Activity);
            _recyclerView.SetLayoutManager(_layoutManager);

            /* Setting up the adapter */
            var users = InteractiveTimetable.Current.UserManager.GetUsers().
                                             OrderBy(x => x.LastName).
                                             ToList();
            _userListAdapter = new UserListAdapter(Activity, users);
            _userListAdapter.ItemClick += OnItemClick;
            _recyclerView.SetAdapter(_userListAdapter);

            /* Determining wide screen device */
            var layout = Activity.FindViewById<LinearLayout>(Resource.Id.main_landscape);
            var _isWideScreenDevice = layout != null && layout.Visibility == ViewStates.Visible;

            if (_isWideScreenDevice)
            {
                ShowUserDetails(users[0].Id);
            }
        }

        public override View OnCreateView(
            LayoutInflater inflater, 
            ViewGroup container, 
            Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.user_list, container, false);
        }

        void OnItemClick(object sender, int userId)
        {   
            Toast.MakeText(Activity, $"This is user with id: {userId}", ToastLength.Short).Show();
        }

        private void ShowUserDetails(int userId)
        {
            var userDetailsFragment = UserDetailsFragment.NewInstance(userId);

            var fragmentManager = FragmentManager.BeginTransaction();
            fragmentManager.Add(Resource.Id.user_details, userDetailsFragment);
            fragmentManager.SetTransition(FragmentTransit.FragmentFade);
            fragmentManager.Commit();
        }
    }
}