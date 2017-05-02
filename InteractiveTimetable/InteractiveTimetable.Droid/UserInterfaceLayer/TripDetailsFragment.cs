using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    public class TripDetailsFragment : Fragment
    {
        #region Constants
        public static readonly string FragmentTag = "trip_details_fragment";
        private static readonly string HospitalTripIdKey = "current_trip_id";
        #endregion

        #region Widgets
        private ImageButton _editButton;
        private Button _tripMonitoringButton;
        #endregion

        #region Events
        public event Action<int> EditButtonClicked;
        public event Action<int, int> TripMonitoringButtonClicked;
        #endregion

        #region Properties
        public int TripId => Arguments.GetInt(HospitalTripIdKey, 0);

        #endregion

        #region Methods

        #region Construct Methods
        public static TripDetailsFragment NewInstance(int tripId)
        {
            var tripDetailsFragment = new TripDetailsFragment()
            {
                Arguments = new Bundle()
            };
            tripDetailsFragment.Arguments.PutInt(HospitalTripIdKey, tripId);

            return tripDetailsFragment;
        }
        #endregion

        #region Event Handlers
        public override View OnCreateView(
            LayoutInflater inflater, 
            ViewGroup container, 
            Bundle savedInstanceState)
        {
            if (container == null)
            {
                return null;
            }

            /* Get data */
            var trip = InteractiveTimetable.Current.HospitalTripManager.GetHospitalTrip(TripId);
            if (trip == null)
            {
                return null;
            }

            var tripView = inflater.Inflate(Resource.Layout.trip_details, container, false);
            
            /* Set start date */
            var startDate = tripView.FindViewById<TextView>(Resource.Id.trip_start_date);
            startDate.Text += " " + trip.StartDate.ToString("d MMMM yyyy, H:mm");

            /* Set finish date */
            var endDate = tripView.FindViewById<TextView>(Resource.Id.trip_end_date);
            endDate.Text += " " + trip.FinishDate.ToString("d MMMM yyy, H:mm");

            /* Set trip number */
            var tripNumber = tripView.FindViewById<TextView>(Resource.Id.trip_number);
            tripNumber.Text += trip.Number;

            /* Set button click handlers */
            _editButton = tripView.FindViewById<ImageButton>(Resource.Id.edit_trip_button);
            _editButton.Click += OnEditTripButtonClicked;

            _tripMonitoringButton = tripView.FindViewById<Button>(Resource.Id.monitoring_button);
            _tripMonitoringButton.Click += OnTripMonitoringButtonClicked;

            return tripView;
        }

        private void OnTripMonitoringButtonClicked(object sender, EventArgs e)
        {
            var trip = InteractiveTimetable.Current.HospitalTripManager.GetHospitalTrip(TripId);
            TripMonitoringButtonClicked?.Invoke(trip.UserId, TripId);
        }

        private void OnEditTripButtonClicked(object sender, EventArgs args)
        {
            EditButtonClicked?.Invoke(TripId);
        }

        public override void OnDestroy()
        {
            _editButton.Click -= OnEditTripButtonClicked;
            GC.Collect();

            base.OnDestroy();
        }
        #endregion

        #endregion
    }
}