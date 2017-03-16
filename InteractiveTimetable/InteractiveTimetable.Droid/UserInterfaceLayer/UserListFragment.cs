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
        public static readonly string FragmentTag = "user_list_fragment";

        private static readonly string UserIdKey = "current_user_id";
        private RecyclerView _recyclerView;
        private RecyclerView.LayoutManager _layoutManager;
        private UserListAdapter _userListAdapter;

        private bool _isWideScreenDevice;
        private int _currentUserId;
        
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
            
            /* Getting users ordered by last name */
            var users = InteractiveTimetable.Current.UserManager.GetUsers().
                                             OrderBy(x => x.LastName).
                                             ToList();

            /* Initializing current user id */
            if (savedInstanceState != null)
            {
                _currentUserId = savedInstanceState.GetInt(UserIdKey, 0);
            }
            else
            {
                _currentUserId = users[0].Id;
            }

            /* Getting views */
            _recyclerView = Activity.FindViewById<RecyclerView>(Resource.Id.user_recycler_view);

            /* Setting up the layout manager */
            _layoutManager = new LinearLayoutManager(Activity);
            _recyclerView.SetLayoutManager(_layoutManager);

            /* Setting up the adapter */
            
            _userListAdapter = new UserListAdapter(Activity, users);
            _userListAdapter.ItemClick += OnItemClick;
            _recyclerView.SetAdapter(_userListAdapter);

            /* Determining wide screen device */
            var layout = Activity.FindViewById<LinearLayout>(Resource.Id.main_landscape);
            _isWideScreenDevice = layout != null && layout.Visibility == ViewStates.Visible;

            if (_isWideScreenDevice)
            {
                ShowUserDetails(_currentUserId);
            }
        }

        public override View OnCreateView(
            LayoutInflater inflater, 
            ViewGroup container, 
            Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.user_list, container, false);
        }

        public void OnItemClick(object sender, int userId)
        {   
            // TODO: Delete the line below when dubugging is done
            Toast.MakeText(Activity, $"This is user with id: {userId}", ToastLength.Short).Show();

            ShowUserDetails(userId);
            _currentUserId = userId;
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutInt(UserIdKey, _currentUserId);
            base.OnSaveInstanceState(outState);
        }

        private void ShowUserDetails(int userId)
        {
            if (_isWideScreenDevice)
            {
                /* Checking what fragment is shown and replacing if needed */
                var userDetails = FragmentManager.FindFragmentByTag(UserDetailsFragment.FragmentTag)
                        as UserDetailsFragment;

                if (userDetails == null || _currentUserId != userId)
                {
                    var userDetailsFragment = UserDetailsFragment.NewInstance(userId);

                    var fragmentManager = FragmentManager.BeginTransaction();
                    fragmentManager.Replace(
                            Resource.Id.user_details, 
                            userDetailsFragment, 
                            UserDetailsFragment.FragmentTag
                        );

                    fragmentManager.SetTransition(FragmentTransit.FragmentFade);
                    fragmentManager.Commit();
                }
            }
            else
            {
                // TODO: launch new activivty
            }
        }
    }
}