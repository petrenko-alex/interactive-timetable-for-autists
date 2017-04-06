using System;
using System.Collections.Generic;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.BusinessLayer.Models;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    class HospitalTripListAdapter : RecyclerView.Adapter
    {
        #region Events
        public event Action<int, int> ItemClick;
        public event Action<int, int> RequestToDeleteTrip;
        #endregion

        #region Properties
        public IList<HospitalTrip> Trips { get; set; }
        public override int ItemCount => Trips.Count;
        #endregion

        #region Internal Variables
        private Activity _context;
        #endregion

        #region Methods

        #region Construct Methods
        public HospitalTripListAdapter(Activity context, IList<HospitalTrip> trips)
        {
            _context = context;
            Trips = trips;
        }
        #endregion

        #region Event Handlers
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var tripAtPosition = Trips[position];
            var tripAsString = _context.GetString(Resource.String.trip_in_list);
            var viewHolder = holder as HospitalTripViewHolder;

            if (viewHolder != null)
            {
                viewHolder.TripName.Text = tripAsString + tripAtPosition.Number;
                viewHolder.TripId = tripAtPosition.UserId;
                viewHolder.PositionInList = position;
            }
        }

        public override RecyclerView.ViewHolder
                OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var inflater = LayoutInflater.From(_context);
            var view = inflater.Inflate(Resource.Layout.trip_list_item, parent, false);

            var holder = new HospitalTripViewHolder(view, OnItemClick, OnItemLongClick);
            return holder;
        }

        private void OnItemLongClick(View view, int userId, int positionInList)
        {
            var menu = new PopupMenu(_context, view);
            menu.Inflate(Resource.Menu.hospital_trip_popup_menu);
            menu.MenuItemClick += (sender, args) =>
            {
                RequestToDeleteTrip?.Invoke(userId, positionInList);
            };

            menu.Show();
        }

        private void OnItemClick(int tripId, int positionInList)
        {
            ItemClick?.Invoke(tripId, positionInList);
        }
        #endregion
        
        #endregion
    }
}