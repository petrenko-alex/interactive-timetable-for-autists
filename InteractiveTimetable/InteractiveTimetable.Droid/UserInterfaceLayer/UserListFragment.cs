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
        private AutoCompleteTextView _findUserField;
        private TextView _emptyListTextView;
        private LinearLayout _userListMainContent;
        #endregion

        #region Internal Variables
        private RecyclerView.LayoutManager _layoutManager;
        private UserListAdapter _userListAdapter;
        private int _currentUserId;
        private IList<User> _users;
        #endregion

        #region Events
        public event Action<int> ListItemClicked;
        public event Action AddUserButtonClicked;
        public event Action NoMoreUsersInList;
        #endregion

        #region Methods

        #region Construct Methods
        public static UserListFragment NewInstance()
        {
            /* Getting users ordered by last name */
            var users = GetUsers();

            var userListFragment = new UserListFragment
            {
                Arguments = new Bundle(),
                _users = users,
            };

            return userListFragment;
        }
        #endregion

        #region Event Handlers
        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            
            /* Initialize current user id */
            int userId = 0;
            if (savedInstanceState != null)
            {
                userId = savedInstanceState.GetInt(UserIdKey, 0);
            }
            else if(_users.Count != 0)
            {
                userId = _users[0].Id;
            }

            /* Get views */
            _recyclerView = Activity.FindViewById<RecyclerView>(Resource.Id.user_recycler_view);

            /* Set up the layout manager */
            _layoutManager = new LinearLayoutManager(Activity);
            _recyclerView.SetLayoutManager(_layoutManager);

            /* Set up the adapter */
            _userListAdapter = new UserListAdapter(Activity, _users);
            _userListAdapter.ItemClick += OnItemClick;
            _userListAdapter.RequestToDeleteUser += OnDeleteButtonClicked;
            _recyclerView.SetAdapter(_userListAdapter);

            /* Set event handlers */
            _addUserBtn = Activity.FindViewById<Button>(Resource.Id.add_user_btn);
            _addUserBtn.Click += OnAddBtnClicked;

            /* Set up the adapter for find user field */
            _findUserField = Activity.FindViewById<AutoCompleteTextView>(Resource.Id.find_user);
            _findUserField.Threshold = 1;
            _findUserField.ItemClick += OnFindUserItemClicked;
            SetAdapterToFindUserField(_users);

            /* Initialize other widgets */
            _userListMainContent = Activity.FindViewById<LinearLayout>(Resource.Id.user_list_main_content);
            _emptyListTextView = Activity.FindViewById<TextView>(Resource.Id.empty_list);

            /* Adjust widgets visibility in case user list is empty */
            SwitchEmptyList();

            /* Initialize class variables */
            _currentUserId = userId;
        }

        private void OnFindUserItemClicked(object sender, AdapterView.ItemClickEventArgs e)
        {
            /* Hide keyboard */
            InteractiveTimetable.Current.HideKeyboard(Activity.CurrentFocus.WindowToken);

            /* Find user by FIO */
            var selectedUser = e.View.FindViewById<TextView>(Android.Resource.Id.Text1).Text;
            var FIO = selectedUser.Split(' ');

            var user = GetUsers().First(
                x => x.LastName == FIO[0] &&
                     x.FirstName == FIO[1] &&
                     x.PatronymicName == FIO[2]);

            if (user != null)
            {
                ListItemClicked?.Invoke(user.Id);
                _currentUserId = user.Id;
            }
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
            _findUserField.ItemClick -= OnFindUserItemClicked;
            _userListAdapter.ItemClick -= OnItemClick;
            _userListAdapter.RequestToDeleteUser -= OnDeleteButtonClicked;
            GC.Collect();

            base.OnDestroy();
        }

        public void OnItemClick(object sender, UserListEventArgs args)
        {
            ListItemClicked?.Invoke(args.UserId);
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
        #endregion

        #region Other Methods
        public int GetFirstUserId()
        {
            return _users[0].Id;
        }

        public void DataSetChanged()
        {
            _userListAdapter.Users = GetUsers();
            _userListAdapter.NotifyDataSetChanged();

            /* Refresh find user field data set */
            SetAdapterToFindUserField(_userListAdapter.Users);
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
                alert.SetNegativeButton(GetString(Resource.String.cancel_button), (sender1, args) => { });

                Dialog dialog = alert.Create();
                dialog.Show();
            }
        }

        private void DeleteUser(int userId, int positionInList)
        {
            /* Delete from database */
            InteractiveTimetable.Current.UserManager.DeleteUser(userId);

            /* Delete from adapter */
            _userListAdapter.RemoveItem(positionInList, _currentUserId == userId);

            /* Refresh find user field data set */
            SetAdapterToFindUserField(_userListAdapter.Users);

            /* Adjust widgets visibility in case user list is empty */
            SwitchEmptyList();
        }

        public void AddUser(int userId)
        {
            /* Insert in adapter */
            _currentUserId = userId;
            int insertedPosition = _userListAdapter.InsertItem(userId);

            /* Scroll to inserted position */
            _layoutManager.ScrollToPosition(insertedPosition);

            /* Refresh find user field data set */
            SetAdapterToFindUserField(_userListAdapter.Users);

            /* Adjust widgets visibility */
            SwitchEmptyList();
        }

        private static IList<User> GetUsers()
        {
            return InteractiveTimetable.Current.UserManager.GetUsers().
                                        OrderBy(x => x.LastName).
                                        ToList();
        }

        private void SetAdapterToFindUserField(IList<User> users)
        {
            var dataForFindField = users.Select(x => x.LastName + " " + x.FirstName + " " + x.PatronymicName).ToList();
            var adapter = new ArrayAdapter<string>(Activity, Android.Resource.Layout.SimpleListItem1, dataForFindField);
            _findUserField.Adapter = adapter;
        }

        private void SwitchEmptyList()
        {
            if (_userListAdapter?.ItemCount == 0)
            {
                _userListMainContent.Visibility = ViewStates.Gone;
                _emptyListTextView.Visibility = ViewStates.Visible;
                NoMoreUsersInList?.Invoke();
            }
            else
            {
                _userListMainContent.Visibility = ViewStates.Visible;
                _emptyListTextView.Visibility = ViewStates.Gone;
            }
        }

        public bool IsListEmpty()
        {
            return _users.Count == 0;
        }
        #endregion

        #endregion
    }
}