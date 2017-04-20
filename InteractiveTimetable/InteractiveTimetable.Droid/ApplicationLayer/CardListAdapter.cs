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
        public event Action<int> AddCardButtonClicked;
        public event Action<int, ImageView> CardSelected;
        public event Action<int, int> RequestToDeleteCard;
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
                    /* Set image */
                    // TODO: Change to normal load - viewHolder.CardImage.SetImageURI(Android.Net.Uri.Parse(cardAtPosition.PhotoPath));
                    var imageSize = ImageHelper.ConvertDpToPixels(
                        140,
                        InteractiveTimetable.Current.ScreenDensity
                    );
                    var bitmap = cardAtPosition.PhotoPath.LoadAndResizeBitmap(imageSize, imageSize);
                    viewHolder.CardImage.SetImageBitmap(bitmap);
                    viewHolder.CardId = cardAtPosition.Id;

                    /* Set frame */
                    viewHolder.CardFrame.SetBackgroundResource(Resource.Drawable.card_frame);
                    var paddingInDp = 5;
                    var paddingInPx = ImageHelper.ConvertDpToPixels(
                        paddingInDp,
                        InteractiveTimetable.Current.ScreenDensity
                    );
                    viewHolder.CardFrame.SetPadding(
                        paddingInPx,
                        paddingInPx,
                        paddingInPx,
                        paddingInPx
                    );
                    viewHolder.CardImage.SetScaleType(ImageView.ScaleType.CenterCrop);
                }
                /* If showing add button */
                else
                {
                    viewHolder.CardFrame.SetBackgroundResource(0);
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

        private void OnCardLongClick(View view, int cardId, int positionInList)
        {
            if (cardId > 0)
            {
                var menu = new PopupMenu(_context, view);
                menu.Inflate(Resource.Menu.card_popup_menu);
                menu.MenuItemClick += (sender, args) =>
                {
                    RequestToDeleteCard?.Invoke(cardId, positionInList);
                };

                menu.Show();
            }
        }

        private void OnCardClick(int cardId, ImageView cardImage)
        {
            /* Add button clicked */
            if (cardId <= 0)
            {
                /* 
                 * Pick any card from data set because we only need card type 
                 * and it's the same for all card in data set 
                 */
                var card = Cards[0];
                AddCardButtonClicked?.Invoke(card.CardTypeId);
            }
            /* Card clicked */
            else
            {
                CardSelected?.Invoke(cardId, cardImage);
            }
        }
        #endregion

        #region Other Methods
        public void RemoveItem(int positionInList)
        {
            /* Remove from adapter data set */
            Cards.RemoveAt(positionInList);

            /* Notify adapter */
            NotifyItemRemoved(positionInList);
            NotifyItemRangeChanged(positionInList, ItemCount);
        }

        public void InsertItem(int cardId)
        {
            /* Get data from database */
            var card = InteractiveTimetable.Current.ScheduleManager.Cards.GetCard(cardId);

            /* Insert in data set */
            int lastPosition = ItemCount - 1;
            Cards.Insert(lastPosition, card);

            /* Notify adapter */
            int amountOfChangedItems = 2;
            NotifyItemInserted(lastPosition);
            NotifyItemRangeChanged(lastPosition, amountOfChangedItems);
        }
        #endregion

        #endregion
    }
}