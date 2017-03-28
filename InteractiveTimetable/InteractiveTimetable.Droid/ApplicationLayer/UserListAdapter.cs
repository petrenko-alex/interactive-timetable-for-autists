using System;
using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using InteractiveTimetable.BusinessLayer.Models;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    public class UserListAdapter : RecyclerView.Adapter
    {
        public event EventHandler<UserListEventArgs> ItemClick;
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

                /* Highliting current item */
                if (CurrentPosition == position)
                {
                    holder.ItemView.SetBackgroundColor(_selectedItemBackground);
                    holder.ItemView.Background.Alpha = 30;
                    holder.ItemView.Selected = true;
                }
                else
                {
                    holder.ItemView.SetBackgroundColor(Color.Transparent);
                }
            }
        }

        public override RecyclerView.ViewHolder 
            OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var inflater = LayoutInflater.From(_context);
            View view = inflater.Inflate(Resource.Layout.user_list_item, parent, false);

            UserViewHolder holder = new UserViewHolder(view, OnClick);
            return holder;
        }

        public override int ItemCount => Users.Count;

        void OnClick(int userId, int positionInList)
        {
            /* Redraw old and new selection */
            NotifyItemChanged(positionInList);
            CurrentPosition = positionInList;
            NotifyItemChanged(positionInList);

            var args = new UserListEventArgs()
            {
                UserId = userId,
                PositionInList = positionInList
            };
            ItemClick?.Invoke(this, args);
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
                /* Delete first in list */
                if (positionInList == 0)
                {
                    CurrentPosition = 0;
                }
                /* Delete last in list */
                else if (positionInList == ItemCount)
                {
                    CurrentPosition = positionInList - 1;
                }
                /* Delete from the middle of the list */
                else
                {
                    CurrentPosition = positionInList;
                }

                OnClick(Users[CurrentPosition].Id, CurrentPosition);
            }
            else
            {
                // TODO: Show another layout when no more users in the list
            }
        }
    }
}