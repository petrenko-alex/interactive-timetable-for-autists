using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    internal class NewTapeItemViewHolder : RecyclerView.ViewHolder
    {
        #region Properties
        public ImageView ItemImage { get; }
        public FrameLayout ItemFrame { get; }
        public ImageButton DeleteButton { get; }
        public int TapeItemId { get; set; }
        public int PositionInList { get; set; }
        #endregion

        public NewTapeItemViewHolder(
            View itemView,
            Action<NewTapeItemViewHolder> listenerForClick,
            Action<int> listenerForDeleteButtonClick)
            : base(itemView)
        {
            ItemImage = itemView.FindViewById<ImageView>(Resource.Id.nti_image);
            ItemFrame = itemView.FindViewById<FrameLayout>(Resource.Id.nti_frame);
            DeleteButton = itemView.FindViewById<ImageButton>(Resource.Id.nti_delete_button);

            ItemImage.Click += (sender, e) => listenerForClick(this);
            DeleteButton.Click += (sender, e) => listenerForDeleteButtonClick(PositionInList);
        }
    }
}