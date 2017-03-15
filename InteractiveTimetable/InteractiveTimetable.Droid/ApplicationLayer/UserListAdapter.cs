using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    public class UserListAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;

        public UserManager UserManager;

        public UserListAdapter(UserManager userManager)
        {
            UserManager = userManager;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            UserViewHolder viewHolder = holder as UserViewHolder;


            //viewHolder.LastName.Text = UserManager
        }

        public override RecyclerView.ViewHolder 
            OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var inflater = LayoutInflater.From(parent.Context);
            View view = inflater.Inflate(Resource.Layout.user_list_item, parent, false);

            View itemView = LayoutInflater.From(parent.Context).
                Inflate(Resource.Layout.user_list_item, parent, false);
            

            UserViewHolder holder = new UserViewHolder(itemView, OnClick);
            return holder;
        }

        public override int ItemCount => UserManager.UserCount;

        void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }
    }
}