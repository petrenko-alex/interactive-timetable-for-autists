using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.BusinessLayer.Models;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    public class UserListAdapter : RecyclerView.Adapter
    {
        #region Constants
        private static readonly int UserImageSizeDp = 150;
        #endregion

        #region Events
        public event EventHandler<UserListEventArgs> ItemClick;
        public event Action<int, int> RequestToDeleteUser;
        #endregion

        #region Properties
        public IList<User> Users { get; set; }
        public override int ItemCount => Users.Count;
        #endregion

        #region Internal Variables
        private Activity _context;
        #endregion

        #region Methods

        #region Construct Methods
        public UserListAdapter(Activity context, IList<User> users)
        {
            _context = context;
            Users = users;
        }
        #endregion

        #region Event Handlers
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var userAtPosition = Users[position];
            var viewHolder = holder as UserViewHolder;

            if (viewHolder != null)
            {
                viewHolder.LastName.Text = userAtPosition.LastName;
                viewHolder.FirstAndPatronymicName.Text = userAtPosition.FirstName +
                                                         " " +
                                                         userAtPosition.PatronymicName;

                var imageSize = ImageHelper.ConvertDpToPixels(UserImageSizeDp);
                var bitmap = userAtPosition.PhotoPath.LoadAndResizeBitmap(imageSize, imageSize);
                if (bitmap != null)
                {
                    viewHolder.UserPhoto.SetImageBitmap(bitmap);
                }

                viewHolder.UserId = userAtPosition.Id;
                viewHolder.PositionInList = position;
            }
        }

        public override RecyclerView.ViewHolder
            OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var inflater = LayoutInflater.From(_context);
            var view = inflater.Inflate(Resource.Layout.user_list_item, parent, false);

            var holder = new UserViewHolder(view, OnClick, OnItemLongClick);
            return holder;
        }

        private void OnClick(int userId, int positionInList)
        {
            var args = new UserListEventArgs()
            {
                UserId = userId,
                PositionInList = positionInList
            };
            ItemClick?.Invoke(this, args);
        }

        private void OnItemLongClick(View view, int userId, int positionInList)
        {
            var menu = new PopupMenu(_context, view);
            menu.Inflate(Resource.Menu.user_popup_menu);
            menu.MenuItemClick += (sender, args) =>
            {
                RequestToDeleteUser?.Invoke(userId, positionInList);
            };

            menu.Show();
        }
        #endregion

        #region Other Methods
        public void RemoveItem(int positionInList, bool isCurrentUser)
        {
            /* Remove from adapter data set */
            Users.RemoveAt(positionInList);

            /* Notify adapter about item removing */
            NotifyItemRemoved(positionInList);
            NotifyItemRangeChanged(positionInList, Users.Count);

            /* Adjust list selection */
            if (ItemCount != 0)
            {
                /* If current user is deleted */
                if (isCurrentUser)
                {
                    /* Delete last in list */
                    if (positionInList == ItemCount)
                    {
                        positionInList -= 1;
                    }

                    OnClick(Users[positionInList].Id, positionInList);
                }
            }
        }

        public int InsertItem(int userId)
        {
            /* Get actual information from database */
            var users = InteractiveTimetable.Current.UserManager.GetUsers().
                                             OrderBy(x => x.LastName).
                                             ToList();
            var user = InteractiveTimetable.Current.UserManager.GetUser(userId);

            /* Find the place to insert */
            int positionToInsert = 0;
            int userCount = users.Count;
            for (int i = 0; i < userCount; ++i)
            {
                if (user.LastName.Equals(users[i].LastName))
                {
                    positionToInsert = i;
                }
            }

            /* Insert in data set */
            Users.Insert(positionToInsert, user);

            /* Notify adapter */
            NotifyItemInserted(positionToInsert);

            return positionToInsert;
        }
        #endregion

        #endregion
    }
}