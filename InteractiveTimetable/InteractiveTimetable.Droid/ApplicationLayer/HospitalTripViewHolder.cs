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
    class HospitalTripViewHolder : RecyclerView.ViewHolder
    {
        #region Properties
        public TextView TripName { get; }
        public int TripId { get; set; }
        public int PositionInList { get; set; }
        #endregion

        public HospitalTripViewHolder(
            View itemView,
            Action<int, int> listenerForClick,
            Action<View, int, int> listenerForLongPress)
            : base(itemView)
        {
            TripName = itemView.FindViewById<TextView>(Resource.Id.trip_list_item);

            itemView.Click += (sender, e) => listenerForClick(TripId, PositionInList);
            itemView.LongClick += (sender, e) => listenerForLongPress(itemView, TripId, PositionInList);
        }
    }
}