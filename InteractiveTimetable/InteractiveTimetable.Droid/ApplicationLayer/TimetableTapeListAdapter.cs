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
using InteractiveTimetable.BusinessLayer.Models;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    class TimetableTapeListAdapter : RecyclerView.Adapter
    {
        #region Events
        public event Action<int, int> ItemClick;
        #endregion

        #region Properties
        public IList<Card> TapeItems { get; set; }
        public override int ItemCount => TapeItems.Count;
        #endregion

        #region Internal Variables
        private Activity _context;
        #endregion

        #region Methods

        #region Construct Methods
        public TimetableTapeListAdapter(Activity context, IList<Card> tapeItems)
        {
            _context = context;
            TapeItems = tapeItems;
        }
        #endregion

        #region Event Handlers
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var tapeItemAtPosition = TapeItems[position];
            var viewHolder = holder as TimetableTapeItemViewHolder;

            if (viewHolder != null)
            {
                viewHolder.ItemImage.SetImageURI(Android.Net.Uri.Parse(tapeItemAtPosition.PhotoPath));
                viewHolder.TapeItemId = tapeItemAtPosition.Id;
                viewHolder.PositionInList = position;
            }
        }

        public override RecyclerView.ViewHolder
            OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var inflater = LayoutInflater.From(_context);
            var view = inflater.Inflate(Resource.Layout.timetable_tape_item, parent, false);

            var holder = new TimetableTapeItemViewHolder(view, OnItemClick, OnItemLongClick);
            return holder;
        }

        private void OnItemLongClick(View arg1, int arg2, int arg3)
        {
            // TODO: Cancel activity completion
        }

        private void OnItemClick(View view, int arg2, int arg3)
        {
            // TODO: Activity completed - show animation 
        }
        #endregion

        #endregion
    }
}