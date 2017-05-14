using System;
using System.Collections.Generic;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.BusinessLayer.Models;
using PopupMenu = Android.Support.V7.Widget.PopupMenu;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    public class CardListAdapter : RecyclerView.Adapter
    {
        #region Constants
        private static readonly int CardImageSizeDp = 140;
        #endregion

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
        private int _cardTypeId;
        #endregion

        #region Methods

        #region Construct Methods
        public CardListAdapter(Activity context, IList<Card> cards, int cardTypeId)
        {
            _context = context;
            _cardTypeId = cardTypeId;
            Cards = cards;
        }
        #endregion

        #region Event Handlers
        public override async void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var cardAtPosition = Cards[position];
            var viewHolder = holder as CardViewHolder;

            if (viewHolder != null)
            {
                /* If showing card */
                if (cardAtPosition.Id > 0)
                {
                    /* Set image */
                    var imageSize = ImageHelper.ConvertDpToPixels(CardImageSizeDp);
                    var bitmap = await cardAtPosition.PhotoPath.LoadScaledDownBitmapForDisplayAsync(
                        imageSize,
                        imageSize
                    );
                    if (bitmap != null)
                    {
                        viewHolder.CardImage.SetImageBitmap(bitmap);
                    }
                    viewHolder.CardId = cardAtPosition.Id;

                    /* Set frame */
                    viewHolder.CardFrame.SetBackgroundResource(Resource.Drawable.grey_frame_round5);
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
                AddCardButtonClicked?.Invoke(_cardTypeId);
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