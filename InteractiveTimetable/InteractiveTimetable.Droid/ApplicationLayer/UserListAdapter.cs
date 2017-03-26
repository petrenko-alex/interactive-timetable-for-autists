using System;
using System.Collections.Generic;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using InteractiveTimetable.BusinessLayer.Models;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    public class UserListAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;
        private Activity _context;
        public IList<User> Users { get; set; }

        public UserListAdapter(Activity context, IList<User> users)
        {
            _context = context;
            Users = users;
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

        void OnClick(int userId)
        {
            ItemClick?.Invoke(this, userId);
        }
    }
}