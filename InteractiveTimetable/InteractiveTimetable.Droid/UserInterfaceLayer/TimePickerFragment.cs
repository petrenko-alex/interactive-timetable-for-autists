using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    public class TimePickerFragment : DialogFragment,
                                      TimePickerDialog.IOnTimeSetListener
    {
        #region Constants
        public static readonly string FragmentTag = "time_picker_fragment";
        #endregion

        #region Delegates
        private Action<DateTime> _timeSelectedHandler = delegate { };
        #endregion

        #region Internal Variables
        private DateTime _currentTime;
        #endregion

        #region Methods

        #region Construct Methods
        public static TimePickerFragment NewInstance(
            DateTime currentTime,
            Action<DateTime> onTimeSelected)
        {
            var fragment = new TimePickerFragment
            {
                _timeSelectedHandler = onTimeSelected,
                _currentTime = currentTime
            };

            return fragment;
        }
        #endregion

        #region EventHandlers
        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            var dialog = new TimePickerDialog(
                Activity,
                this,
                _currentTime.Hour,
                _currentTime.Minute,
                true);
            dialog.SetCanceledOnTouchOutside(true);

            return dialog;
        }

        public void OnTimeSet(TimePicker view, int hourOfDay, int minute)
        {
            var today = DateTime.Today;
            var selectedTime = new DateTime(
                today.Year,
                today.Month,
                today.Day,
                hourOfDay,
                minute,
                0
            );
            _timeSelectedHandler(selectedTime);
        }

        public override void OnCancel(IDialogInterface dialog)
        {
            base.OnCancel(dialog);
            _timeSelectedHandler(_currentTime);
        }
        #endregion

        #endregion
    }
}