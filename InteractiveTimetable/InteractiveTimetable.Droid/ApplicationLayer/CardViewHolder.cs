using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    internal class CardViewHolder : RecyclerView.ViewHolder
    {
        #region Properties
        public ImageView CardImage { get; }
        public FrameLayout CardFrame { get; }
        public int CardId { get; set; }
        public int PositionInList { get; set; }
        #endregion

        public CardViewHolder(
            View itemView,
            Action<int, ImageView> listenerForClick,
            Action<int, int> listenerForLongClick)
            : base(itemView)
        {
            CardImage = itemView.FindViewById<ImageView>(Resource.Id.card_list_item_image);
            CardFrame = itemView.FindViewById<FrameLayout>(Resource.Id.card_list_item_frame);

            itemView.Click += (sender, e) => listenerForClick(CardId, CardImage);
            itemView.LongClick += (sender, e) => listenerForLongClick(CardId, PositionInList);
        }
    }
}