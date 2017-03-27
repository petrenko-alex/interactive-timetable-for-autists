using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    class UserViewHolder : RecyclerView.ViewHolder
    {
        public ImageView UserPhoto { get; private set; }
        public TextView LastName { get; private set; }
        public TextView FirstAndPatronymicName { get; private set; }
        public int UserId { get; set; }
        public int PositionInList { get; set; }

        public UserViewHolder(View itemView, Action<int, int> listener) : base(itemView)
        {
            UserPhoto = itemView.FindViewById<ImageView>(Resource.Id.user_photo);
            LastName = itemView.FindViewById<TextView>(Resource.Id.user_lastname);
            FirstAndPatronymicName 
                = itemView.FindViewById<TextView>(Resource.Id.user_fpname);

            itemView.Click += (sender, e) => listener(UserId, PositionInList);
        }
    }
}