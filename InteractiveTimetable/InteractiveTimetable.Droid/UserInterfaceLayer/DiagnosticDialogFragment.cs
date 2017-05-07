using System;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Michaelmuenzer.Android.Scrollablennumberpicker;
using InteractiveTimetable.BusinessLayer.Models;

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
        #endregion

        #region Widgets
        private Button _applyButton;
        private Button _cancelButton;
        private EditText _diagnosticDate;
        private EditText _diagnosticTime;
        #endregion

        #region Internal Variables
        private int _diagnosticId;
        private Diagnostic _diagnostic;
        private DateTime _diagnosticDateTime;
        #endregion

        public static DiagnosticDialogFragment NewInstance(int diagnosticId)
        {
            var fragment = new DiagnosticDialogFragment();

            var args = new Bundle();
            args.PutInt("diagnostic_id", diagnosticId);
            fragment.Arguments = args;

            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _diagnosticId = Arguments.GetInt("diagnostic_id");
            if (_diagnosticId > 0)
            {
                _diagnostic = InteractiveTimetable.Current.DiagnosticManager.
                                                   GetDiagnostic(_diagnosticId);
            }

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
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
            throw new NotImplementedException();
        }

        private void OnApplyButtonClicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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