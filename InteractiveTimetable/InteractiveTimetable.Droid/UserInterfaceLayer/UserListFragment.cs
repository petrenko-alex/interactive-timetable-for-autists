using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.Droid.ApplicationLayer;
using Android.Content;
using InteractiveTimetable.BusinessLayer.Models;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    public class UserListFragment : Fragment
    {
        #region Constants
        public static readonly string FragmentTag = "user_list_fragment";
        private static readonly string UserIdKey = "current_user_id";
        #endregion

        #region Widgets
        private RecyclerView _recyclerView;
        private Button _addUserBtn;
        #endregion

        #region Internal Variables
        private RecyclerView.LayoutManager _layoutManager;
        private UserListAdapter _userListAdapter;
        private int _currentUserId;
        #endregion

        public event Action<int> ListItemClicked;
        public event Action AddUserButtonClicked;
        
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
            var users = GetUsers();

            /* Initializing current user id */
            int userId;
            if (savedInstanceState != null)
            {
                userId = savedInstanceState.GetInt(UserIdKey, 0);
            }
            else
            {
                userId = users[0].Id;
            }

            /* Getting views */
            _recyclerView = Activity.FindViewById<RecyclerView>(Resource.Id.user_recycler_view);

            /* Setting up the layout manager */
            _layoutManager = new LinearLayoutManager(Activity);
            _recyclerView.SetLayoutManager(_layoutManager);

            /* Setting up the adapter */
            _userListAdapter = new UserListAdapter(Activity, users);
            _userListAdapter.ItemClick += OnItemClick;
            _userListAdapter.RequestToDeleteUser += OnDeleteButtonClicked;
            _recyclerView.SetAdapter(_userListAdapter);

            /* Setting event handlers */
            _addUserBtn = Activity.FindViewById<Button>(Resource.Id.add_user_btn);
            _addUserBtn.Click += OnAddBtnClicked;

            /* Initializing class variables */
            _currentUserId = userId;
        }

        public override View OnCreateView(
            LayoutInflater inflater, 
            ViewGroup container, 
            Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.user_list, container, false);
        }

        public override void OnDestroy()
        {
            _addUserBtn.Click -= OnAddBtnClicked;
            GC.Collect();

            base.OnDestroy();
        }

        public void OnItemClick(object sender, UserListEventArgs args)
        {   
            // TODO: Delete the line below when dubugging is done
            Toast.MakeText(Activity, $"This is user with id: {args.UserId}", ToastLength.Short).Show();

            ShowUserDetails(args.UserId);
            _currentUserId = args.UserId;
        }

        public void OnAddBtnClicked(object sender, EventArgs args)
        {
            AddUserButtonClicked?.Invoke();
        }
        

        public void OnDeleteButtonClicked(int userId, int positionInList)
        {
            /* Show alert if user in current timetable */
            if (InteractiveTimetable.Current.UserManager.IsUserInPresentTimetable(userId))
            {
                AskAndDeleteUser(
                    GetString(Resource.String.user_in_present_timetable),
                    userId,
                    positionInList);
            }
            /* Show general alert */
            else
            {
                AskAndDeleteUser(
                    GetString(Resource.String.sure_to_delete_user),
                    userId,
                    positionInList);
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutInt(UserIdKey, _currentUserId);
            base.OnSaveInstanceState(outState);
        }

        private void ShowUserDetails(int userId)
        {
            ListItemClicked?.Invoke(userId);
        }

        private IList<User> GetUsers()
        {
            return InteractiveTimetable.Current.UserManager.GetUsers().
                                        OrderBy(x => x.LastName).
                                        ToList();
        }

        public void DataSetChanged()
        {
            _userListAdapter.Users = GetUsers();
            _userListAdapter.NotifyDataSetChanged();
        }

        private void AskAndDeleteUser(string questionToAsk, int userId, int positionInList)
        {
            using (var alert = new AlertDialog.Builder(Activity))
            {
                alert.SetTitle(GetString(Resource.String.delete_user));
                alert.SetMessage(questionToAsk);
                alert.SetPositiveButton(GetString(Resource.String.delete_button), (sender1, args) =>
                {
                    DeleteUser(userId, positionInList);
                });
                alert.SetNegativeButton(GetString(Resource.String.cancel_button), (sender1, args) => {});

                Dialog dialog = alert.Create();
                dialog.Show();
            }
        }

        private void DeleteUser(int userId, int positionInList)
        {
            /* Delete from database */
            InteractiveTimetable.Current.UserManager.DeleteUser(userId);

            /* Delete from adapter */
            _userListAdapter.RemoveItem(positionInList);
        }

        public int GetFirstUserId()
        {
            return GetUsers()[0].Id;
        }
    }
}