using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    public class DatePickerFragment : DialogFragment,
                                      DatePickerDialog.IOnDateSetListener
    {
        #region Constants
        public static readonly string FragmentTag = "date_picker_fragment";
        #endregion

        #region Delegates
        private Action<DateTime> _dateSelectedHandler = delegate { };
        #endregion

        #region Internal Variables
        private DateTime _currentDate;
        private DateTime _minDate;
        #endregion

        #region Methods

        #region Construct Methods
        public static DatePickerFragment NewInstance(
            DateTime currentDate,
            Action<DateTime> onDateSelected,
            DateTime minDate)
        {
            var fragment = new DatePickerFragment
            {
                _dateSelectedHandler = onDateSelected,
                _currentDate = currentDate,
                _minDate = minDate
            };

            return fragment;
        }
        #endregion

        #region Event Handlers
        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            var dialog = new DatePickerDialog(
                Activity,
                this,
                _currentDate.Year,
                _currentDate.Month - 1,
                _currentDate.Day
            );
            dialog.SetCanceledOnTouchOutside(true);
            dialog.DatePicker.MinDate =
                    (long) (_minDate.Date - new DateTime(1970, 1, 1)).TotalMilliseconds;

            return dialog;
        }

        public void OnDateSet(DatePicker view, int year, int month, int day)
        {
            if (view.IsShown)
            {
                var selectedDate = new DateTime(year, month + 1, day);
                _dateSelectedHandler(selectedDate);
            }   
        }
        #endregion

        #endregion
    }
}