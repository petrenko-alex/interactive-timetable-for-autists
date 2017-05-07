using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.Droid.ApplicationLayer;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "MonitoringActivity", MainLauncher = true)]
    public class MonitoringActivity : 
        ActionBarActivity, 
        ViewTreeObserver.IOnGlobalLayoutListener,
        IDiagnosticDialogListener
    {
        #region Constants
        private static readonly int HeaderColumnWidth = 150;
        private static readonly int HeaderColumnHeight = 50;
        private static readonly int GradeColumnWidth = 50;
        private static readonly int GradeColumnHeight = 50;
        private static readonly string TickUnicodeSymbol = "\u2713";
        private static readonly int MaxVisibleDiagnostics = 3;
        #endregion

        #region Widgets
        private LinearLayout _layoutForTable;
        private ImageButton _backButton;
        private ImageButton _homeButton;
        private ImageButton _nextTablePageButton;
        private ImageButton _previousTablePageButton;
        private ImageButton _addDiagnosticButton;
        private TableLayout _headerTable;
        #endregion

        #region Internal Variables
        private User _user;
        private HospitalTrip _trip;
        private List<Diagnostic> _diagnostics;
        private List<TableLayout> _tables;
        private List<int> _visibleDiagnosticIndexes;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.monitoring);
            OverridePendingTransition(
                Resource.Animation.enter_from_right,
                Resource.Animation.exit_to_left
            );

            /* Get views */
            var tripInfo = FindViewById<TextView>(Resource.Id.monitoring_trip_info);
            var heading = FindViewById<TextView>(Resource.Id.monitoring_header);
            _layoutForTable = FindViewById<LinearLayout>(Resource.Id.table_layout);
            _nextTablePageButton = FindViewById<ImageButton>(Resource.Id.next_table);
            _previousTablePageButton = FindViewById<ImageButton>(Resource.Id.previous_table);
            _addDiagnosticButton = FindViewById<ImageButton>(Resource.Id.add_diagnostic_button);

            /* Set handlers */
            _nextTablePageButton.Click += OnNextTablePageButtonClicked;
            _previousTablePageButton.Click += OnPreviousTablePageButtonClicked;
            _addDiagnosticButton.Click += OnAddDiagnosticButtonClicked;

            /* Get data */
            var userId = Intent.GetIntExtra("user_id", 0);
            var tripId = Intent.GetIntExtra("trip_id", 0);

            // TODO: Delete after finish
            var debugUser = InteractiveTimetable.Current.UserManager.GetUsers().ToList()[2];
            userId = debugUser.Id;
            tripId = debugUser.HospitalTrips[1].Id;

            /* Set data to views */
            if (tripId > 0)
            {
                _trip = InteractiveTimetable.Current.HospitalTripManager.GetHospitalTrip(tripId);
                string text = $"{GetString(Resource.String.trip_in_list)}" +
                              $"{_trip.Number} " +
                              $"{_trip.StartDate:dd.MM.yyyy} - {_trip.FinishDate:dd.MM.yyyy}";
                tripInfo.Text = text;

                heading.Text = GetString(Resource.String.trip_monitoring);
            }
            else
            {
                tripInfo.Visibility = ViewStates.Gone;
                heading.Text = GetString(Resource.String.general_monitoring);
            }

            if (userId > 0)
            {
                _user = InteractiveTimetable.Current.UserManager.GetUser(userId);
                heading.Text = $"{_user.FirstName} - {heading.Text}";
            }

            /* Set tool bar */
            var toolbar = FindViewById<Toolbar>(Resource.Id.monitoring_toolbar);
            SetSupportActionBar(toolbar);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            AdjustToolbarForActivity();

            /* Get diagnostics and create table */
            _diagnostics = new List<Diagnostic>();
            _visibleDiagnosticIndexes = new List<int>();
            if (_trip != null)
            {
                _diagnostics = _trip.Diagnostics.OrderBy(x => x.Date).ToList();
            }
            else
            {
                var trips = _user.HospitalTrips;
                foreach (var trip in trips)
                {
                    _diagnostics.AddRange(trip.Diagnostics);
                }
                _diagnostics = _diagnostics.OrderBy(x => x.Date).ToList();
            }

            if (_diagnostics.Count > 0)
            {
                CreateTables();
            }
            else
            {
                // TODO: Show info screen
            }
        }

        private void OnAddDiagnosticButtonClicked(object sender, EventArgs e)
        {
            var transaction = FragmentManager.BeginTransaction();
            var previousFragment = FragmentManager.
                    FindFragmentByTag(DiagnosticDialogFragment.FragmentTag);

            if (previousFragment != null)
            {
                transaction.Remove(previousFragment);
            }
            transaction.AddToBackStack(null);

            var dialog = DiagnosticDialogFragment.NewInstance(0, _trip.Id);
            dialog.Show(transaction, DiagnosticDialogFragment.FragmentTag);

        }

        private void OnPreviousTablePageButtonClicked(object sender, EventArgs e)
        {
            /* Check if has diagnostics */
            int firstVisibleDiagnosticIndex = _visibleDiagnosticIndexes[0];
            if (firstVisibleDiagnosticIndex > 0)
            {
                /* Decrement indexes in _visibleDiagnosticIndexes */
                for (int i = 0; i < MaxVisibleDiagnostics; ++i)
                {
                    _visibleDiagnosticIndexes[i]--;
                }

                /* Set tables */
                for (int i = 0; i < MaxVisibleDiagnostics; ++i)
                {
                    int index = _visibleDiagnosticIndexes[i];
                    var table = _tables[index];

                    /* Remove old table from layout */
                    _layoutForTable.RemoveViewAt(i + 1);

                    /* Set another table to layout */
                    _layoutForTable.AddView(table, i + 1);
                }

                /* Enable next button because one was hidden */
                _nextTablePageButton.Visibility = ViewStates.Visible;

                /* Disable previous button if no more diagnostics to show */
                firstVisibleDiagnosticIndex = _visibleDiagnosticIndexes[0];
                if (firstVisibleDiagnosticIndex == 0)
                {
                    _previousTablePageButton.Visibility = ViewStates.Invisible;
                }
            }
        }

        private void OnNextTablePageButtonClicked(object sender, EventArgs e)
        {
            /* Check if has diagnostics */
            int lastVisibleDiagnosticIndex = _visibleDiagnosticIndexes[MaxVisibleDiagnostics - 1];
            if (lastVisibleDiagnosticIndex != _diagnostics.Count - 1)
            {
                /* Increment indexes in _visibleDiagnosticIndexes */
                for (int i = 0; i < MaxVisibleDiagnostics; ++i)
                {
                    _visibleDiagnosticIndexes[i]++;
                }

                /* Delete current visible tables from layout */
                for (int i = MaxVisibleDiagnostics; i > 0; --i)
                {
                    var table = _layoutForTable.GetChildAt(i);
                    _layoutForTable.RemoveView(table);
                }

                /* Set tables */
                for (int i = 0; i < MaxVisibleDiagnostics; ++i)
                {
                    /* If already has table for diagnostic */
                    int index = _visibleDiagnosticIndexes[i];
                    var table = _tables[index];

                    if (table != null)
                    {
                        /* Set another table to layout */
                        _layoutForTable.AddView(table, i + 1);
                    }
                    else
                    {
                        /* Need to create new table */
                        AddDiagnosticTable(index, i + 1);
                    }
                }

                /* Enable previous button because one was hidden */
                _previousTablePageButton.Visibility = ViewStates.Visible;

                /* Disable next button if no more diagnostics to show */
                lastVisibleDiagnosticIndex = _visibleDiagnosticIndexes[MaxVisibleDiagnostics - 1];
                if (lastVisibleDiagnosticIndex == _diagnostics.Count - 1)
                {
                    _nextTablePageButton.Visibility = ViewStates.Invisible;
                }
            }
        }

        private void AdjustToolbarForActivity()
        {
            /* Set toolbar layout */
            var toolbar = FindViewById<Toolbar>(Resource.Id.monitoring_toolbar);
            var toolbarContent = FindViewById<LinearLayout>(Resource.Id.toolbar_content);
            var layout = LayoutInflater.Inflate(Resource.Layout.monitoring_toolbar, toolbar, false);
            toolbarContent.AddView(layout);

            /* Set toolbar controls */
            var title = toolbar.FindViewById<TextView>(Resource.Id.toolbar_title);
            title.Text = GetString(Resource.String.monitoring);

            var clock = toolbar.FindViewById<TextClock>(Resource.Id.toolbar_clock);
            clock.Format24Hour = InteractiveTimetable.DateTimeFormat;

            _backButton = toolbar.FindViewById<ImageButton>(Resource.Id.toolbar_back);
            _backButton.Click += OnBackButtonClicked;

            _homeButton = toolbar.FindViewById<ImageButton>(Resource.Id.toolbar_home);
            _homeButton.Click += OnHomeButtonClicked;

        }
        
        private void OnHomeButtonClicked(object sender, EventArgs e)
        {
            Finish();

            /* Call home screen activity */
            var intent = new Intent(this, typeof(HomeScreenActivity));
            intent.SetFlags(ActivityFlags.ClearTop);
            StartActivity(intent);
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
        {
            Finish();

            OverridePendingTransition(
                Resource.Animation.enter_from_left,
                Resource.Animation.exit_to_right
            );
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();

            OverridePendingTransition(
                Resource.Animation.enter_from_left,
                Resource.Animation.exit_to_right
            );
        }

        public void OnGlobalLayout()
        {
            _layoutForTable.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);

            /* Adjust table controls width to match table width */
            var width = _layoutForTable.Width;
            var tableControls = FindViewById<RelativeLayout>(Resource.Id.table_controls);
            tableControls.LayoutParameters.Width = width;
        }

        public void OnNewDiagnostiAdded(int diagnosticId)
        {
            /* Get just added diagnostic */
            var diagnostic = InteractiveTimetable.Current.DiagnosticManager.
                                                  GetDiagnostic(diagnosticId);

            /* Find place for new diagnostic */
            int amountOfDiagnostics = _diagnostics.Count;
            int newDiagnosticIndex = amountOfDiagnostics;
            for (int i = 0; i < amountOfDiagnostics; ++i)
            {
                if (diagnostic.Date < _diagnostics[i].Date)
                {
                    newDiagnosticIndex = i;
                }
            }

            /* Insert new diagnostic in _diagnostics data set */
            _diagnostics.Insert(newDiagnosticIndex, diagnostic);

            
            int firstVisibleDiagnosticIndex = _visibleDiagnosticIndexes[0];
            int lastVisibleDiagnosticIndex = _visibleDiagnosticIndexes[MaxVisibleDiagnostics - 1];
            if (newDiagnosticIndex < firstVisibleDiagnosticIndex)
            {
                /* 
                 * If diagnostic was inserted to position before current visible diagnostics, 
                 * increment indexes of current visible 
                 */
                for (int i = 0; i < MaxVisibleDiagnostics; ++i)
                {
                    _visibleDiagnosticIndexes[i]++;
                }
            }
            else if (newDiagnosticIndex >= firstVisibleDiagnosticIndex &&
                     newDiagnosticIndex <= lastVisibleDiagnosticIndex)
            {
                /* If need to repaint tables to show table for new diagnostic */
                /* Delete current visible tables from layout */
                for (int i = MaxVisibleDiagnostics; i > 0; --i)
                {
                    var table = _layoutForTable.GetChildAt(i);
                    _layoutForTable.RemoveView(table);
                }

                /* Set tables */
                for (int i = 0; i < MaxVisibleDiagnostics; ++i)
                {
                    int index = _visibleDiagnosticIndexes[i];

                    /* If already has table for diagnostic */
                    if (index <= _tables.Count - 1)
                    {
                        var table = _tables[index];

                        /* Set another table to layout */
                        _layoutForTable.AddView(table, i + 1);
                    }
                    else
                    {
                        /* Need to create new table */
                        AddDiagnosticTable(index, i + 1);
                    }
                }
            }
                
            

            /* Show table if need */
            for (int i = 0; i < MaxVisibleDiagnostics; ++i)
            {
                int index = _visibleDiagnosticIndexes[i];
            }
        }

        #region Table Methods
        private void CreateTables()
        {
            /* Create header table */
            AddHeaderTable();

            /* First initialize with null's to fit _diagnostics size */
            int diagnosticsAmount = _diagnostics.Count;
            _tables = new List<TableLayout>();
            for (int i = 0; i < diagnosticsAmount; ++i)
            {
                _tables.Add(null);
            }

            /* Create tables for diagnostics */
            if (diagnosticsAmount > MaxVisibleDiagnostics)
            {
                diagnosticsAmount = MaxVisibleDiagnostics;
            }
            for (int i = 0; i < diagnosticsAmount; ++i)
            {
                AddDiagnosticTable(i);
                _visibleDiagnosticIndexes.Add(i);
            }

            _layoutForTable.ViewTreeObserver.AddOnGlobalLayoutListener(this);

            /* Adjust table page control buttons */
            _previousTablePageButton.Visibility = ViewStates.Invisible;

            if (_diagnostics.Count <= MaxVisibleDiagnostics)
            {
                _nextTablePageButton.Visibility = ViewStates.Invisible;
            }
        }

        private void AddHeaderTable()
        {
            _headerTable = CreateTable();

            /* Get data */
            var criterions = InteractiveTimetable.Current.DiagnosticManager.GetCriterions().ToList();

            /* Prepare layout params */
            var paramsForDefinitions = new TableRow.LayoutParams(
                HeaderColumnWidth,
                HeaderColumnHeight
            );

            /* Create and insert first header column */
            var topLeftColumn = CreateColumn(
                paramsForDefinitions,
                GetString(Resource.String.criterions_and_grades)
            );

            var firstRow = CreateRow();
            firstRow.AddView(topLeftColumn);
            _headerTable.AddView(firstRow);

            /* Create and insert second header column */
            var dateColumn = CreateColumn(
                paramsForDefinitions,
                GetString(Resource.String.date)
            );

            var secondRow = CreateRow();
            secondRow.AddView(dateColumn);
            _headerTable.AddView(secondRow);

            /* Add criterion definition header columns */
            foreach (var criterion in criterions)
            {
                /* Create column for criterion definition */
                var column = CreateColumn(paramsForDefinitions, criterion);

                /* Add to table */
                var row = CreateRow();
                row.AddView(column);
                _headerTable.AddView(row);
            }

            /* Add columns for sums */
            var partialSumsColumns = CreateColumn(
                paramsForDefinitions,
                GetString(Resource.String.grade_partial_sum)
            );
            var sumColumn = CreateColumn(
                paramsForDefinitions,
                GetString(Resource.String.grade_total_sum)
            );

            var lastRow1 = CreateRow();
            lastRow1.AddView(partialSumsColumns);
            _headerTable.AddView(lastRow1);

            var lastRow2 = CreateRow();
            lastRow2.AddView(sumColumn);
            _headerTable.AddView(lastRow2);

            /* Add table to layout */
            _layoutForTable.AddView(_headerTable);
        }

        private void AddDiagnosticTable(int diagnosticNumber, int insertTableAt = -1)
        {
            /* Prepare data */
            var gradesAmount = 4;

            var paramsForTable = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent
            );
            int marginDp = 10;
            int marginPx = ImageHelper.ConvertDpToPixels(marginDp);
            paramsForTable.SetMargins(marginPx, 0, marginDp, 0);

            var paramsForDiagnostic = new TableRow.LayoutParams(
                GradeColumnWidth,
                GradeColumnHeight
            );

            var diagnostic = _diagnostics[diagnosticNumber];

            /* Create table */
            var table = CreateTable(paramsForTable);
            table.SetBackgroundResource(Resource.Drawable.table_frame);
            var paddingDp = 4;
            var paddingPx = ImageHelper.ConvertDpToPixels(paddingDp);
            table.SetPadding(paddingDp, paddingPx, paddingPx, paddingPx);

            /* Create grade header column */
            var firstRow = CreateRow();
            for (int i = 0; i < gradesAmount; ++i)
            {
                var gradeHeaderColumn = CreateColumn(paramsForDiagnostic, $"{i + 1}");
                SetColumnColor(i + 1, gradeHeaderColumn);
                firstRow.AddView(gradeHeaderColumn);
            }
            table.AddView(firstRow);

            /* Create date column */
            var secondRow = CreateRow();
            var paramsForDate = new TableRow.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                GradeColumnHeight
            )
            {
                Span = 4
            };

            var dateColumn = CreateColumn(
                paramsForDate,
                diagnostic.Date.ToString("dd.MM.yyyy")
            );
            secondRow.AddView(dateColumn);
            table.AddView(secondRow);

            /* Create grade columns */
            var paramsForGrade = new TableRow.LayoutParams(
                GradeColumnWidth,
                GradeColumnHeight
            );

            foreach (var grade in diagnostic.CriterionGrades)
            {
                /* Create 4 columns for grades */
                var currentRow = CreateRow();
                for (int i = 0; i < gradesAmount; ++i)
                {
                    var gradeColumn = CreateColumn(paramsForGrade, "");
                    currentRow.AddView(gradeColumn);
                }

                if (grade.Grade == "1")
                {
                    var column = (TextView) currentRow.GetChildAt(0);
                    column.Text = grade.Grade;
                    column.SetBackgroundResource(Resource.Drawable.table_grade_1_frame);
                }
                else if (grade.Grade == "2")
                {
                    var column = (TextView)currentRow.GetChildAt(1);
                    column.Text = grade.Grade;
                    column.SetBackgroundResource(Resource.Drawable.table_grade_2_frame);
                }
                else if (grade.Grade == "3")
                {
                    var column = (TextView)currentRow.GetChildAt(2);
                    column.Text = grade.Grade;
                    column.SetBackgroundResource(Resource.Drawable.table_grade_3_frame);
                }
                else if (grade.Grade == "4")
                {
                    var column = (TextView)currentRow.GetChildAt(3);
                    column.Text = grade.Grade;
                    column.SetBackgroundResource(Resource.Drawable.table_grade_4_frame);
                }
                else if (grade.Grade.Length == 4)
                {
                    /* Parse complex grade */
                    for (int i = 0; i < gradesAmount; ++i)
                    {
                        if (grade.Grade[i] == '1')
                        {
                            var column = (TextView)currentRow.GetChildAt(i);
                            column.Text = TickUnicodeSymbol;
                        }
                    }
                }
                
                table.AddView(currentRow);
            }

            /* Set partial sums */
            var partialSumRow = CreateRow();
            for (int i = 0; i < gradesAmount; ++i)
            {
                int sum = InteractiveTimetable.Current.DiagnosticManager.
                                               GetPartialSum(diagnostic.Id, i + 1);
                var sumColumn = CreateColumn(paramsForGrade, sum + "");
                partialSumRow.AddView(sumColumn);
            }
            table.AddView(partialSumRow);

            /* Set total sum */
            var totalSumRow = CreateRow();
            int totalSum = InteractiveTimetable.Current.DiagnosticManager.GetTotalSum(diagnostic.Id);
            var totalSumColumn = CreateColumn(paramsForDate, totalSum + "");
            totalSumRow.AddView(totalSumColumn);
            table.AddView(totalSumRow);
            
            /* Add table to data set */
            _tables[diagnosticNumber] = table;

            /* Add table to layout */
            if (insertTableAt >= 0)
            {
                _layoutForTable.AddView(table, insertTableAt);
            }
            else
            {
                _layoutForTable.AddView(table);
            }
        }

        private TableLayout CreateTable(LinearLayout.LayoutParams layoutParams = null)
        {
            if (layoutParams == null)
            {
                layoutParams = new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.MatchParent
                );
            }

            var table = new TableLayout(this)
            {
                LayoutParameters = layoutParams
            };


            return table;
        }

        private TableRow CreateRow()
        {
            var tableParams = new TableLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent
            );

            var row = new TableRow(this)
            {
                LayoutParameters = tableParams
            };

            return row;
        }

        private TextView CreateColumn(TableRow.LayoutParams layoutParams, string columnText)
        {
            /* Create column with parameters */
            var column = new TextView(this)
            {
                LayoutParameters = layoutParams,
            };
            column.SetBackgroundResource(Resource.Drawable.table_column_frame);
            column.Gravity = GravityFlags.Center;

            var paddingInDp = ImageHelper.ConvertPixelsToDp(5);
            column.SetPadding(paddingInDp, 0, paddingInDp, 0);

            /* Set text */
            column.Text = columnText;

            return column;
        }

        private void SetColumnColor(int grade, TextView column)
        {
            if (grade == 1)
            {
                column.SetBackgroundResource(Resource.Drawable.table_grade_1_frame);
            }
            else if (grade == 2)
            {
                column.SetBackgroundResource(Resource.Drawable.table_grade_2_frame);
            }
            else if (grade == 3)
            {
                column.SetBackgroundResource(Resource.Drawable.table_grade_3_frame);

            }
            else if (grade == 4)
            {
                column.SetBackgroundResource(Resource.Drawable.table_grade_4_frame);
            }
        }
        #endregion
    }
}