using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.Droid.ApplicationLayer;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    public class HospitalTripListFragment : Fragment
    {
        #region Constants
        public static readonly string FragmentTag = "hospital_trip_list_fragment";
        private static readonly string HospitalTripIdKey = "current_trip_id";
        #endregion

        #region Widgets
        private RecyclerView _recyclerView;
        private Button _addTripBtn;
        private TextView _emptyListTextView;
        private LinearLayout _tripListMainContent;
        #endregion

        #region Internal Variables
        private RecyclerView.LayoutManager _layoutManager;
        private HospitalTripListAdapter _tripListAdapter;
        private int _currentTripId;
        private IList<HospitalTrip> _trips;
        private int _userId;
        #endregion

        #region Events
        public event Action<int> ListItemClicked;
        public event Action AddTripButtonClicked;
        public event Action NoMoreTripsInList;
        #endregion

        #region Methods

        #region Construct Methods
        public static HospitalTripListFragment NewInstance(int userId)
        {
            /* Getting trips ordered by trip number */
            var trips = GetTrips(userId);

            var hospitalTripListFragment = new HospitalTripListFragment()
            {
                _trips = trips,
                _userId = userId
            };

            return hospitalTripListFragment;
        }
        #endregion

        #region Event Handlers
        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            /* Initializing current trip id */
            int tripId = 0;
            if (savedInstanceState != null)
            {
                tripId = savedInstanceState.GetInt(HospitalTripIdKey, 0);
            }
            else if (_trips.Count != 0)
            {
                tripId = _trips[0].Id;
            }

            /* Getting views */
            _recyclerView = Activity.FindViewById<RecyclerView>(Resource.Id.trip_recycler_view);
            _tripListMainContent = Activity.FindViewById<LinearLayout>(Resource.Id.hospital_trip_main_content);
            _emptyListTextView = Activity.FindViewById<TextView>(Resource.Id.empty_trip_list);

            /* Setting up the layout manager */
            _layoutManager = new LinearLayoutManager(Activity);
            _recyclerView.SetLayoutManager(_layoutManager);

            /* Setting up the adapter */
            SetAdapter();

            /* Setting event handlers */
            _addTripBtn = Activity.FindViewById<Button>(Resource.Id.add_trip_btn);
            _addTripBtn.Click += OnAddButtonClicked;

            /* Adjust widgets visibility in case trip list is empty */
            SwitchEmptyList();

            /* Initializing class variables */
            _currentTripId = tripId;
        }

        public override View OnCreateView(
            LayoutInflater inflater,
            ViewGroup container,
            Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.hospital_trip_list, container, false);
        }

        public override void OnDestroy()
        {
            _addTripBtn.Click -= OnAddButtonClicked;
            _tripListAdapter.ItemClick -= OnItemClick;
            _tripListAdapter.RequestToDeleteTrip -= OnDeleteButtonClicked;
            GC.Collect();

            base.OnDestroy();
        }

        private void OnAddButtonClicked(object sender, EventArgs e)
        {
            AddTripButtonClicked?.Invoke();
        }

        private void OnDeleteButtonClicked(int tripId, int positionInList)
        {
            /* Show alert if trip is present */
            if (InteractiveTimetable.Current.HospitalTripManager.IsHospitalTripPresent(tripId))
            {
                AskAndDeleteTrip(
                    GetString(Resource.String.present_trip),
                    tripId,
                    positionInList
                );
            }
            /* Show general alert */
            else
            {
                AskAndDeleteTrip(
                    GetString(Resource.String.sure_to_delete_trip),
                    tripId,
                    positionInList
                );
            }
        }

        private void AskAndDeleteTrip(string questionToAsk, int tripId, int positionInList)
        {
            using (var alert = new AlertDialog.Builder(Activity))
            {
                alert.SetTitle(GetString(Resource.String.delete_trip));
                alert.SetMessage(questionToAsk);
                alert.SetPositiveButton(GetString(Resource.String.delete_button), (sender1, args) =>
                {
                    DeleteTrip(tripId, positionInList);
                });
                alert.SetNegativeButton(GetString(Resource.String.cancel_button), (sender1, args) => { });

                Dialog dialog = alert.Create();
                dialog.Show();
            }
        }

        private void OnItemClick(int tripId, int positionInList)
        {
            ListItemClicked?.Invoke(tripId);
            _currentTripId = tripId;
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutInt(HospitalTripIdKey, _currentTripId);
            base.OnSaveInstanceState(outState);
        }
        #endregion

        #region Other Methods

        public void DataSetChanged()
        {
            _trips = GetTrips(_userId);
            _tripListAdapter.Trips = _trips;
            _tripListAdapter.NotifyDataSetChanged();
        }

        private void DeleteTrip(int tripId, int positionInList)
        {
            /* Delete from database */
            InteractiveTimetable.Current.HospitalTripManager.DeleteHospitalTrip(tripId);

            /* Refresh adapter */
            DataSetChanged();

            /* Adjust selection */
            if (_tripListAdapter.ItemCount != 0)
            {
                if (_currentTripId == tripId)
                {
                    /* Delete last in list */
                    if (positionInList == _tripListAdapter.ItemCount)
                    {
                        positionInList -= 1;
                    }

                    OnItemClick(_trips[positionInList].Id, positionInList);
                }
            }

            /* Adjust widgets visibility in case trip list is empty */
            SwitchEmptyList();
        }

        public void AddTrip(int tripId)
        {

        }

        public int GetFirstTripId()
        {
            return _trips.Count != 0 ? _trips[0].Id : 0;
        }

        private static IList<HospitalTrip> GetTrips(int userId)
        {
            return InteractiveTimetable.Current.HospitalTripManager.GetHospitalTrips(userId).
                                        OrderBy(x => x.Number).
                                        ToList();
        }

        private void SwitchEmptyList()
        {
            if (_tripListAdapter?.ItemCount == 0)
            {
                _tripListMainContent.Visibility = ViewStates.Gone;
                _emptyListTextView.Visibility = ViewStates.Visible;
                NoMoreTripsInList?.Invoke();
            }
            else
            {
                _tripListMainContent.Visibility = ViewStates.Visible;
                _emptyListTextView.Visibility = ViewStates.Gone;
            }
        }

        public bool IsListEmpty()
        {
            return _trips.Count == 0;
        }

        private void SetAdapter( )
        {
            _tripListAdapter = new HospitalTripListAdapter(Activity, _trips);
            _tripListAdapter.ItemClick += OnItemClick;
            _tripListAdapter.RequestToDeleteTrip += OnDeleteButtonClicked;
            _recyclerView.SetAdapter(_tripListAdapter);
        }
        #endregion

        #endregion
    }
}