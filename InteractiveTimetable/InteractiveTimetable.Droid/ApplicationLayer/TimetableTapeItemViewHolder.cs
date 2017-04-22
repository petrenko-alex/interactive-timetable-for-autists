using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    internal class TimetableTapeItemViewHolder : RecyclerView.ViewHolder
    {
        #region Properties
        public ImageView ItemImage { get; }
        public FrameLayout ItemFrame { get; }
        public int TapeItemId { get; set; }
        public int PositionInList { get; set; }
        #endregion

        public TimetableTapeItemViewHolder(
            View itemView,
            Action<TimetableTapeItemViewHolder, int, int> listenerForClick,
            Action<TimetableTapeItemViewHolder, int, int> listenerForLongClick)
            : base(itemView)
        {
            ItemImage = itemView.FindViewById<ImageView>(Resource.Id.tape_item_image);
            ItemFrame = itemView.FindViewById<FrameLayout>(Resource.Id.tape_item_frame);

            itemView.Click += (sender, e) =>
                listenerForClick(this, TapeItemId, PositionInList);

            itemView.LongClick += (sender, e) =>
                listenerForLongClick(this, TapeItemId, PositionInList);
        }
    }
}