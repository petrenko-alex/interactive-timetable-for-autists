using System;
using System.Collections.Generic;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.BusinessLayer.Models;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    public class CardListAdapter : RecyclerView.Adapter
    {
        #region Events
        public event Action<int, ImageView> ItemClick;
        public event Action<int, int> RequestToDeleteItem;
        #endregion

        #region Properties
        public IList<Card> Cards { get; set; }
        public override int ItemCount => Cards.Count;
        #endregion

        #region Internal Variables
        private Activity _context;
        #endregion

        #region Methods

        #region Construct Methods
        public CardListAdapter(Activity context, IList<Card> cards)
        {
            _context = context;
            Cards = cards;
        }
        #endregion

        #region Event Handlers
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var cardAtPosition = Cards[position];
            var viewHolder = holder as CardViewHolder;

            if (viewHolder != null)
            {
                /* If showing card */
                if (cardAtPosition.Id > 0)
                {
                    // TODO: Change to normal load - viewHolder.CardImage.SetImageURI(Android.Net.Uri.Parse(cardAtPosition.PhotoPath));
                    var imageSize = ImageHelper.ConvertDpToPixels(
                        140,
                        InteractiveTimetable.Current.ScreenDensity
                    );
                    var bitmap = cardAtPosition.PhotoPath.LoadAndResizeBitmap(imageSize, imageSize);
                    viewHolder.CardImage.SetImageBitmap(bitmap);
                    viewHolder.CardId = cardAtPosition.Id;

                    viewHolder.CardImage.SetBackgroundResource(Resource.Drawable.card_frame);
                    var paddingInPx = ImageHelper.ConvertDpToPixels(
                        5,
                        InteractiveTimetable.Current.ScreenDensity
                    );
                    viewHolder.CardImage.SetPadding(paddingInPx, paddingInPx ,paddingInPx, paddingInPx);
                }
                /* If showing add button */
                else
                {
                    viewHolder.ItemView.SetBackgroundResource(0);
                    viewHolder.CardImage.SetImageResource(Resource.Drawable.add_card_button);
                }
                viewHolder.PositionInList = position;
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var inflater = LayoutInflater.From(_context);
            var view = inflater.Inflate(Resource.Layout.card_list_item, parent, false);

            var holder = new CardViewHolder(view, OnCardClick, OnCardLongClick);
            return holder;
        }

        private void OnCardLongClick(int cardId, int positionInList)
        {
            // TODO: Delete card
        }

        private void OnCardClick(int cardID, ImageView cardImage)
        {
            // TODO: Send signal - card selected
        }
        #endregion

        #region Other Methods
        public void RemoveItem(int positionInList)
        {
            
        }

        public int InsertItem(int cardId)
        {
            return 0;
        }
        #endregion

        #endregion
    }
}