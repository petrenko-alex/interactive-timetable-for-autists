using System;
using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.BusinessLayer.Models;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    public class UserListAdapter : RecyclerView.Adapter
    {
        public event EventHandler<UserListEventArgs> ItemClick;
        public event Action<int, int> RequestToDeleteUser;

        private Activity _context;
        public IList<User> Users { get; set; }
        
        public int CurrentPosition { get; set; }
        private readonly Color _selectedItemBackground;

        public UserListAdapter(Activity context, IList<User> users)
        {
            _context = context;
            Users = users;
            CurrentPosition = 0;
            _selectedItemBackground = Color.ParseColor(ImageHelper.HexFrameColor);
        }

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
                viewHolder.UserPhoto.SetImageURI(Android.Net.Uri.Parse(userAtPosition.PhotoPath));
                viewHolder.UserId = userAtPosition.Id;
                viewHolder.PositionInList = position;
            }
        }

        public override RecyclerView.ViewHolder 
            OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var inflater = LayoutInflater.From(_context);
            View view = inflater.Inflate(Resource.Layout.user_list_item, parent, false);

            UserViewHolder holder = new UserViewHolder(view, OnClick, OnItemLongClick);
            return holder;
        }

        public override int ItemCount => Users.Count;

        private void OnClick(int userId, int positionInList)
        {
            CurrentPosition = positionInList;

            var args = new UserListEventArgs()
            {
                UserId = userId,
                PositionInList = positionInList
            };
            ItemClick?.Invoke(this, args);
        }

        private void OnItemLongClick(View view, int userId, int positionInList)
        {
            PopupMenu menu = new PopupMenu(_context, view);
            menu.Inflate(Resource.Menu.user_popup_menu);
            menu.MenuItemClick += (sender, args) =>
            {
                RequestToDeleteUser?.Invoke(userId, positionInList);
            };

            menu.Show();
        }

        public void RemoveItem(int positionInList)
        {
            /* Remove from adapter data set */
            Users.RemoveAt(positionInList);

            /* Notify adapter about item removing */
            NotifyItemRemoved(positionInList);
            NotifyItemRangeChanged(positionInList, Users.Count);

            /* Adjust list selection */
            if (ItemCount != 0)
            {
                /* If deleting user who is in focus now, change the focus */
                if (CurrentPosition == positionInList)
                {
                    /* Delete last in list */
                    if (positionInList == ItemCount)
                    {
                        positionInList -= 1;
                    }

                    OnClick(Users[positionInList].Id, positionInList);
                }
                /* If deleting the user above current user, just adjust current position */
                else if(CurrentPosition > positionInList)
                {
                    CurrentPosition -= 1;
                }

                Console.WriteLine(CurrentPosition);
            }
            else
            {
                // TODO: Show another layout when no more users in the list
            }
        }
    }
}