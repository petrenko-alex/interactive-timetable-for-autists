using System;
using System.Collections.Generic;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    class NewTapeAdapter : RecyclerView.Adapter
    {
        #region Events
        public event Action<int> ItemClick;
        #endregion

        #region Properties
        public int CurrentCard { get; set; }
        public IList<int> TapeItems { get; set; }
        public override int ItemCount => TapeItems.Count;
        #endregion

        #region Internal Variables
        private Activity _context;
        #endregion

        #region Methods

        #region Construct Methods
        public NewTapeAdapter(Activity context, IList<int> tapeItems)
        {
            _context = context;
            if (tapeItems.Count == 0)
            {
                TapeItems = new List<int>()
                {
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0
                };
            }
            else
            {
                TapeItems = tapeItems;
            }
        }
        #endregion

        #region Event Handlers
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as NewTapeItemViewHolder;

            if (viewHolder != null)
            {
                var itemAtPosition = TapeItems[position];

                if (itemAtPosition <= 0)
                {
                    /* Empty item */
                    viewHolder.ItemImage.SetImageResource(Resource.Drawable.empty_new_tape_item);
                }
                else
                {
                    var card = InteractiveTimetable.Current.ScheduleManager.Cards.
                                                    GetCard(itemAtPosition);

                    if (card != null)
                    {
                        /* Set card image */
                        var imageSize = ImageHelper.ConvertDpToPixels(
                            140,
                            InteractiveTimetable.Current.ScreenDensity
                        );
                        var bitmap = card.PhotoPath.LoadAndResizeBitmap(imageSize, imageSize);
                        viewHolder.ItemImage.SetImageBitmap(bitmap);
                    }
                }

                /* Set common properties */
                viewHolder.TapeItemId = itemAtPosition;
                viewHolder.PositionInList = position;
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var inflater = LayoutInflater.From(_context);
            var view = inflater.Inflate(Resource.Layout.new_tape_item, parent, false);

            var holder = new NewTapeItemViewHolder(view, OnItemClick, OnDeleteButtonClicked);
            return holder;
        }

        private void OnDeleteButtonClicked(int positionInList)
        {
            Console.WriteLine("Delete tape item!");
        }

        private void OnItemClick(int positionInList)
        {
            Console.WriteLine("Select tape item!");
        }
        #endregion

        #endregion
    }
}