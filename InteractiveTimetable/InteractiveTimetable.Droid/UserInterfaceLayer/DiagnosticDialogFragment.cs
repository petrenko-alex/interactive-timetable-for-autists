using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Michaelmuenzer.Android.Scrollablennumberpicker;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.Droid.ApplicationLayer;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    public class DiagnosticDialogFragment : DialogFragment
    {
        #region Constants
        public static readonly string FragmentTag = "diagnostic_dialog_fragment";
        private static readonly string DateFormat = "dd.MM.yyyy";
        private static readonly string TimeFormat = "H:mm";
        private static readonly int MinGrade = 1;
        private static readonly int MaxGrade = 4;
        private static readonly int ErrorMessageXOffset = 0;
        private static readonly int ErrorMessageYOffset = 0;
        #endregion

        #region Widgets
        private Button _applyButton;
        private Button _cancelButton;
        private EditText _diagnosticDate;
        private EditText _diagnosticTime;
        #endregion

        #region Internal Variables
        private int _diagnosticId;
        private int _tripId;
        private Diagnostic _diagnostic;
        private DateTime _diagnosticDateTime;
        #endregion

        #region Events
        private IDiagnosticDialogListener _listener;
        #endregion

        public static DiagnosticDialogFragment NewInstance(int diagnosticId, int tripId)
        {
            var fragment = new DiagnosticDialogFragment();

            var args = new Bundle();
            args.PutInt("diagnostic_id", diagnosticId);
            args.PutInt("trip_id", tripId);
            fragment.Arguments = args;

            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _diagnosticId = Arguments.GetInt("diagnostic_id");
            _tripId = Arguments.GetInt("trip_id");
            if (_diagnosticId > 0)
            {
                _diagnostic = InteractiveTimetable.Current.DiagnosticManager.
                                                   GetDiagnostic(_diagnosticId);
            }
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);

            /* Verify that the host activity implements interface */
            _listener = Activity as IDiagnosticDialogListener;
            if (_listener == null)
            {
                throw new InvalidCastException(Activity.ToString() +
                                               " must implement IDiagnosticDialogListener"
                );
            }
        }

        public override View OnCreateView(
            LayoutInflater inflater, 
            ViewGroup container, 
            Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.diagnostic_dialog, container, false);

            /* Get views */
            _applyButton = view.FindViewById<Button>(Resource.Id.dd_apply_button);
            _cancelButton = view.FindViewById<Button>(Resource.Id.dd_cancel_button);
            _diagnosticDate = view.FindViewById<EditText>(Resource.Id.diagnostic_date);
            _diagnosticTime = view.FindViewById<EditText>(Resource.Id.diagnostic_time);

            /* Set handlers */
            _applyButton.Click += OnApplyButtonClicked;
            _cancelButton.Click += OnCancelButtonClicked;
            _diagnosticDate.Click += OnDateClicked;
            _diagnosticTime.Click += OnTimeClicked;

            /* Set data to widgets */
            if (_diagnostic != null)
            {
                LoadDiagnostic(view);

            }
            else
            {
                InitializeForNewDiagnostic();
            }

            return view;
        }

        public override void OnDestroy()
        {
            _applyButton.Click -= OnApplyButtonClicked;
            _cancelButton.Click -= OnCancelButtonClicked;
            _diagnosticDate.Click -= OnDateClicked;
            _diagnosticTime.Click -= OnTimeClicked;

            _diagnostic = null;

            base.OnDestroy();
        }

        private void OnTimeClicked(object sender, EventArgs e)
        {
            var fragment = TimePickerFragment.NewInstance(
                _diagnosticDateTime,
                delegate (DateTime time)
                {
                    var timeSpan = new TimeSpan(time.Hour, time.Minute, time.Second);
                    _diagnosticDateTime = _diagnosticDateTime.Date + timeSpan;
                    _diagnosticTime.Text = _diagnosticDateTime.ToString(TimeFormat);
                });

            fragment.Show(FragmentManager, TimePickerFragment.FragmentTag);
        }

        private void OnDateClicked(object sender, EventArgs e)
        {
            var fragment = DatePickerFragment.NewInstance(
                _diagnosticDateTime,
                delegate (DateTime date)
                {
                    var time = new TimeSpan(
                        _diagnosticDateTime.Hour,
                        _diagnosticDateTime.Minute,
                        _diagnosticDateTime.Second
                    );
                    _diagnosticDateTime = date.Date + time;
                    _diagnosticDate.Text = date.ToString(DateFormat);
                });

            fragment.Show(FragmentManager, DatePickerFragment.FragmentTag);
        }

        private void OnCancelButtonClicked(object sender, EventArgs e)
        {
            Dismiss();
        }

        private void OnApplyButtonClicked(object sender, EventArgs e)
        {
            int numberOfGrades = 17;
            var keys = InteractiveTimetable.Current.DiagnosticManager.GetCriterions().ToList();
            var criterionAndGrades = new Dictionary<string, string>();
            
            /* Get point grades */
            for (int i = 0; i < numberOfGrades; ++i)
            {
                string gradeId = "grade_" + (i + 1);
                int id = Activity.Resources.GetIdentifier(gradeId, "id", Activity.PackageName);
                var gradeView = View.FindViewById<ScrollableNumberPicker>(id);

                criterionAndGrades.Add(keys[i], gradeView.Value + "");
            }

            /* Get tick grade */
            string lastGrade = "";
            int tickGradeLength = 4;
            for (int i = 0; i < tickGradeLength; ++i)
            {
                string gradeId = "grade_18_" + (i + 1);
                int id = Activity.Resources.GetIdentifier(gradeId, "id", Activity.PackageName);
                var gradeView = View.FindViewById<CheckBox>(id);

                if (gradeView.Checked)
                {
                    lastGrade += '1';
                }
                else
                {
                    lastGrade += '0';
                }
            }
            criterionAndGrades.Add(keys.Last(), lastGrade);

            /* Try to save to db */
            try
            {
                int diagnosticId = InteractiveTimetable.Current.DiagnosticManager.SaveDiagnostic(
                    _tripId,
                    _diagnosticDateTime,
                    criterionAndGrades
                );

                
                _listener.OnNewDiagnostiAdded(0);
                Dismiss();
            }
            catch (ArgumentException exception)
            {
                /* Show error message */
                var toast = ToastHelper.GetErrorToast(Activity, exception.Message);
                toast.SetGravity(
                    GravityFlags.ClipVertical,
                    ErrorMessageXOffset,
                    ErrorMessageYOffset
                );
                toast.Show();
                return;
            }
        }

        private void LoadDiagnostic(View view)
        {
            /* Get data */
            var grades = _diagnostic.CriterionGrades.Select(x => x.Grade).ToList();

            /* Set data to widgets */
            Dialog.SetTitle(GetString(Resource.String.editing_diagnostic));
            _applyButton.Text = GetString(Resource.String.edit_button);

            /* Set dates */
            _diagnosticDate.Text = _diagnostic.Date.ToString(DateFormat);
            _diagnosticTime.Text = _diagnostic.Date.ToString(TimeFormat);
            _diagnosticDateTime = _diagnostic.Date;

            /* Set point grades */
            int numberOfGrades = 17;
            for (int i = 0; i < numberOfGrades; ++i)
            {
                string gradeId = "grade_" + (i + 1);
                int id = Activity.Resources.GetIdentifier(gradeId, "id", Activity.PackageName);
                var gradeView = view.FindViewById<ScrollableNumberPicker>(id);
                gradeView.Value = int.Parse(grades[i]);
            }

            /* Set tick grade */
            string lastGrade = grades.Last();
            for (int i = 0; i < lastGrade.Length; ++i)
            {
                string gradeId = "grade_18_" + (i + 1);

                if (lastGrade[i] == '1')
                {
                    int id = Activity.Resources.GetIdentifier(gradeId, "id", Activity.PackageName);
                    var gradeView = view.FindViewById<CheckBox>(id);
                    gradeView.Checked = true;
                }
            }
        }

        private void InitializeForNewDiagnostic()
        {
            /* Set titles */
            Dialog.SetTitle(GetString(Resource.String.adding_diagnostic));

            /* Set dates */
            var now = DateTime.Now;
            _diagnosticDate.Text = now.ToString(DateFormat);
            _diagnosticTime.Text = now.ToString(TimeFormat);
            _diagnosticDateTime = now;
        }
    }
}