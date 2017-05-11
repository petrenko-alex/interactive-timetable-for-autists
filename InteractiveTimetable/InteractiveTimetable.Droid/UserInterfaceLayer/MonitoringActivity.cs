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
using AlertDialog = Android.App.AlertDialog;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "MonitoringActivity", MainLauncher = false)]
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
        private LinearLayout _infoLayout;
        private RelativeLayout _tableControlsLayout;
        private ScrollView _tableScrollView;
        private ImageButton _backButton;
        private ImageButton _homeButton;
        private ImageButton _nextTablePageButton;
        private ImageButton _previousTablePageButton;
        private ImageButton _addDiagnosticButton;
        private TableLayout _headerTable;
        private ImageButton _addFirstDiagnosticButton;
        private ImageButton _showGraphButton;
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
            _infoLayout = FindViewById<LinearLayout>(Resource.Id.monitorin_info_layout);
            _nextTablePageButton = FindViewById<ImageButton>(Resource.Id.next_table);
            _previousTablePageButton = FindViewById<ImageButton>(Resource.Id.previous_table);
            _addDiagnosticButton = FindViewById<ImageButton>(Resource.Id.add_diagnostic_button);
            _tableControlsLayout = FindViewById<RelativeLayout>(Resource.Id.table_controls);
            _tableScrollView = FindViewById<ScrollView>(Resource.Id.table_vertical_scroll);
            _showGraphButton = FindViewById<ImageButton>(Resource.Id.show_graph_button);
            _addFirstDiagnosticButton =
                    FindViewById<ImageButton>(Resource.Id.add_first_diagnostic_button);

            /* Set handlers */
            _nextTablePageButton.Click += OnNextTablePageButtonClicked;
            _previousTablePageButton.Click += OnPreviousTablePageButtonClicked;
            _addDiagnosticButton.Click += OnAddDiagnosticButtonClicked;
            _addFirstDiagnosticButton.Click += OnAddDiagnosticButtonClicked;
            _showGraphButton.Click += OnShowGraphButtonClicked;

            /* Get data */
            var userId = Intent.GetIntExtra("user_id", 0);
            var tripId = Intent.GetIntExtra("trip_id", 0);

            // TODO: Delete after finish
            var debugUser = InteractiveTimetable.Current.UserManager.GetUsers().ToList()[7];
            userId = debugUser.Id;
            tripId = debugUser.HospitalTrips[0].Id;

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
                AdjustVisibilityOfConrolButtons();
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
                AdjustVisibilityOfNoDiagnosticsInfo();
            }

            AdjustVisibilityOfShowGraphButton();
        }

        private void OnShowGraphButtonClicked(object sender, EventArgs e)
        {
            var transaction = FragmentManager.BeginTransaction();
            var previousFragment = FragmentManager.
                    FindFragmentByTag(DiagnosticDialogFragment.FragmentTag);

            if (previousFragment != null)
            {
                transaction.Remove(previousFragment);
            }
            transaction.AddToBackStack(null);

            /* Prepare data for graph */
            var results = new List<int>();
            foreach (var diagnostic in _diagnostics)
            {
                var sum = InteractiveTimetable.Current.DiagnosticManager.GetTotalSum(diagnostic);
                results.Add(sum);
            }
            var dates = _diagnostics.Select(x => x.Date).ToList();

            /* Create and show graph dialog */
            var dialog = GraphDialogFragment.NewInstance(results, dates);
            dialog.Show(transaction, GraphDialogFragment.FragmentTag);
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

        private void OnEditDiagnosticButtonClicked(int diagnosticId)
        {
            var transaction = FragmentManager.BeginTransaction();
            var previousFragment = FragmentManager.
                    FindFragmentByTag(DiagnosticDialogFragment.FragmentTag);

            if (previousFragment != null)
            {
                transaction.Remove(previousFragment);
            }
            transaction.AddToBackStack(null);

            var dialog = DiagnosticDialogFragment.NewInstance(diagnosticId, _trip.Id);
            dialog.Show(transaction, DiagnosticDialogFragment.FragmentTag);
        }

        private void OnDeleteDiagnosticButtonClicked(int diagnosticId)
        {
            using (var alert = new AlertDialog.Builder(this))
            {
                alert.SetTitle(GetString(Resource.String.delete_diagnostic));
                alert.SetMessage(GetString(Resource.String.sure_to_delete_diagnostic));
                alert.SetPositiveButton(GetString(Resource.String.delete_button), (sender1, args) =>
                {
                    DeleteDiagnostic(diagnosticId);
                });
                alert.SetNegativeButton(GetString(Resource.String.cancel_button), (sender1, args) => { });

                Dialog dialog = alert.Create();
                dialog.Show();
            }
        }

        private void DeleteDiagnostic(int diagnosticId)
        {
            /* Get diagnostic by id */
            var diagnostic = _diagnostics.First(x => x.Id == diagnosticId);
            int index = _diagnostics.IndexOf(diagnostic);

            /* Delete from database */
            InteractiveTimetable.Current.DiagnosticManager.DeleteDiagnostic(diagnosticId);

            /* Delete from data sets */
            _diagnostics.RemoveAt(index);
            _tables.RemoveAt(index);

            /* Delete from layout */
            var table = _layoutForTable.GetChildAt(index + 1);
            _layoutForTable.RemoveView(table);

            int visibleAmount = _visibleDiagnosticIndexes.Count;
            int lastVisible = _visibleDiagnosticIndexes[visibleAmount - 1];
            if (lastVisible == _diagnostics.Count)
            {
                /* Create new visible list */
                _visibleDiagnosticIndexes.Clear();
                for (int i = _diagnostics.Count - 1, j = 0; i >= 0; --i, ++j)
                {
                    if (j < MaxVisibleDiagnostics)
                    {
                        InsertOrReplaceInVisibleList(j, i);
                    }
                }

                _visibleDiagnosticIndexes = _visibleDiagnosticIndexes.OrderBy(x => x).ToList();
            }

            if (_diagnostics.Count == 0)
            {
                AdjustVisibilityOfNoDiagnosticsInfo();
            }
            else
            {
                RebuildVisibleTables();
                AsjustVisibilityOfTablePageButtons();
                AdjustVisibilityOfShowGraphButton();
            }
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

                    if (table != null)
                    {
                        _layoutForTable.AddView(table, i + 1);
                    }
                    else
                    {
                        AddDiagnosticTable(index, i + 1);
                    }
                }

                AsjustVisibilityOfTablePageButtons();
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

                AsjustVisibilityOfTablePageButtons();
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

        public void OnNewDiagnosticAdded(int diagnosticId)
        {
            if (_diagnostics.Count == 0)
            {
                AddFirstDiagnostic();
                return;
            }

            /* Get just added diagnostic */
            var diagnostic = InteractiveTimetable.Current.DiagnosticManager.
                                                  GetDiagnostic(diagnosticId);

            int newDiagnosticIndex = FindPlaceForNewDiagnotic(diagnostic);

            /* Insert new diagnostic in _diagnostics data set */
            _diagnostics.Insert(newDiagnosticIndex, diagnostic);

            /* Insert null table to _tables data set */
            _tables.Insert(newDiagnosticIndex, null);

            /* Rebuild _visibleDiagnosticIndexes */
            RebuildVisibleList(newDiagnosticIndex);
            
            RebuildVisibleTables();

            AdjustVisibilityOfNoDiagnosticsInfo();
            AsjustVisibilityOfTablePageButtons();
            AdjustVisibilityOfShowGraphButton();
        }

        public void OnDiagnosticEdited(int diagnosticId)
        {
            /* Get just edited diagnostic */
            var diagnostic = InteractiveTimetable.Current.DiagnosticManager.
                                                  GetDiagnostic(diagnosticId);

            var previousDiagnosticData = _diagnostics.First(x => x.Id == diagnostic.Id);
            bool datesEqual = diagnostic.Date.Equals(previousDiagnosticData.Date);
            if (!datesEqual)
            {
                /* Delete old diagnostic data from data sets */
                int index = _diagnostics.IndexOf(previousDiagnosticData);
                _diagnostics.RemoveAt(index);
                _tables.RemoveAt(index);

                /* Insert in new position */
                int newDiagnosticIndex = FindPlaceForNewDiagnotic(diagnostic);
                _diagnostics.Insert(newDiagnosticIndex, diagnostic);
                _tables.Insert(newDiagnosticIndex,null);

                /* Rebuild _visibleDiagnosticIndexes */
                RebuildVisibleList(newDiagnosticIndex);

                RebuildVisibleTables();

                AsjustVisibilityOfTablePageButtons();
            }
            else
            {
                /* If date of the diagnostic was not changed */
                int index = _diagnostics.IndexOf(previousDiagnosticData);

                /* Update data sets */
                _diagnostics[index] = diagnostic;
                _tables[index] = null;

                RebuildVisibleTables();
            }
        }

        private void AdjustVisibilityOfNoDiagnosticsInfo()
        {
            if (_diagnostics.Count == 0)
            {
                _infoLayout.Visibility = ViewStates.Visible;
                _tableControlsLayout.Visibility = ViewStates.Gone;
                _tableScrollView.Visibility = ViewStates.Gone;
            }
            else
            {
                _infoLayout.Visibility = ViewStates.Gone;
                _tableControlsLayout.Visibility = ViewStates.Visible;
                _tableScrollView.Visibility = ViewStates.Visible;
            }
        }

        #region Table Methods

        private void AddFirstDiagnostic()
        {
            _trip = InteractiveTimetable.Current.HospitalTripManager.GetHospitalTrip(_trip.Id);
            _diagnostics = _trip.Diagnostics.OrderBy(x => x.Date).ToList();
            CreateTables();
            AdjustVisibilityOfNoDiagnosticsInfo();
        }
    
        private void CreateTables()
        {
            /* If not created yet */
            if (_layoutForTable.ChildCount == 0)
            {
                /* Create header table */
                AddHeaderTable();
            }

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

            if (_trip != null)
            {
                var managementColumn = CreateColumn(
                    paramsForDefinitions,
                    GetString(Resource.String.management)
                );

                var managementRow = CreateRow();
                managementRow.AddView(managementColumn);
                _headerTable.AddView(managementRow);
            }

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

            var paramsForDate = new TableRow.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                GradeColumnHeight
            )
            {
                Span = 4
            };

            var diagnostic = _diagnostics[diagnosticNumber];

            /* Create table */
            var table = CreateTable(paramsForTable);
            table.SetBackgroundResource(Resource.Drawable.table_frame);
            var paddingDp = 4;
            var paddingPx = ImageHelper.ConvertDpToPixels(paddingDp);
            table.SetPadding(paddingDp, paddingPx, paddingPx, paddingPx);

            /* Create row with manage buttons */
            if (_trip != null)
            {
                var paramsForManagement = new TableRow.LayoutParams(
                    ViewGroup.LayoutParams.WrapContent,
                    GradeColumnHeight
                )
                {
                    Span = 4,
                    Gravity = GravityFlags.Center
                };

                var view = LayoutInflater.From(this).Inflate(Resource.Layout.management_column, null);
                view.LayoutParameters = paramsForManagement;

                var editButton = view.FindViewById<ImageButton>(
                    Resource.Id.edit_diagnostic_button
                );
                var deleteButton = view.FindViewById<ImageButton>(
                    Resource.Id.delete_diagnostic_button
                );

                editButton.Click += (sender, e) => OnEditDiagnosticButtonClicked(diagnostic.Id);
                deleteButton.Click += (sender, e) => OnDeleteDiagnosticButtonClicked(diagnostic.Id);

                /* Add to table */
                var manageRow = CreateRow();
                manageRow.AddView(view);
                table.AddView(manageRow);
            }

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
            var dateColumn = CreateColumn(
                paramsForDate,
                diagnostic.Date.ToString("dd.MM.yyyy H:mm")
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

        private void RebuildVisibleTables()
        {
            int amountOfVisible = _visibleDiagnosticIndexes.Count;

            /* Delete current visible tables from layout */
            for (int i = amountOfVisible; i > 0; --i)
            {
                var table = _layoutForTable.GetChildAt(i);
                _layoutForTable.RemoveView(table);
            }

            /* Set new tables */
            for (int i = 0; i < amountOfVisible; ++i)
            {
                /* If already has table for diagnostic */
                int index = _visibleDiagnosticIndexes[i];
                var table = _tables[index];
                if (table != null)
                {
                    _layoutForTable.AddView(table, i + 1);
                }
                else
                {
                    AddDiagnosticTable(index, i + 1);
                }
            }
        }

        private void InsertOrReplaceInVisibleList(int position, int element)
        {
            var listSize = _visibleDiagnosticIndexes.Count;

            if (position == listSize)
            {
                _visibleDiagnosticIndexes.Insert(position, element);
            }
            else
            {
                _visibleDiagnosticIndexes[position] = element;
            }
        }

        private void RebuildVisibleList(int newDiagnosticIndex)
        {
            /* If insert in the middle */
            if (newDiagnosticIndex > 0 &&
                newDiagnosticIndex < (_diagnostics.Count - 1))
            {
                InsertOrReplaceInVisibleList(0, newDiagnosticIndex - 1);
                InsertOrReplaceInVisibleList(1, newDiagnosticIndex);
                InsertOrReplaceInVisibleList(2, newDiagnosticIndex + 1);
            }
            /* If insert as first */
            else if (newDiagnosticIndex == 0)
            {
                InsertOrReplaceInVisibleList(0, newDiagnosticIndex);

                int i = 1;
                while (i < MaxVisibleDiagnostics &&
                       i < _diagnostics.Count)
                {
                    InsertOrReplaceInVisibleList(i, i);
                    i++;
                }
            }
            /* If insert as last */
            else if (newDiagnosticIndex == (_diagnostics.Count - 1))
            {
                if (_diagnostics.Count >= MaxVisibleDiagnostics)
                {
                    InsertOrReplaceInVisibleList(2, newDiagnosticIndex);
                    InsertOrReplaceInVisibleList(1, newDiagnosticIndex - 1);
                    InsertOrReplaceInVisibleList(0, newDiagnosticIndex - 2);
                }
                else if (_diagnostics.Count == MaxVisibleDiagnostics - 1)
                {
                    InsertOrReplaceInVisibleList(0, newDiagnosticIndex - 1);
                    InsertOrReplaceInVisibleList(1, newDiagnosticIndex);
                }
            }
        }

        private void AsjustVisibilityOfTablePageButtons()
        {
            int visibleDiagnosticsAmount = _visibleDiagnosticIndexes.Count;
            int firstVisible = _visibleDiagnosticIndexes[0];
            int lastVisible = _visibleDiagnosticIndexes[visibleDiagnosticsAmount - 1];

            /* Adjust visibility of previous page button */
            if (firstVisible > 0)
            {
                _previousTablePageButton.Visibility = ViewStates.Visible;
            }
            else if (firstVisible == 0)
            {
                _previousTablePageButton.Visibility = ViewStates.Invisible;
            }

            /* Adjust visibility of next page button */
            if (lastVisible < (_diagnostics.Count - 1))
            {
                _nextTablePageButton.Visibility = ViewStates.Visible;
            }
            else if (lastVisible == (_diagnostics.Count - 1))
            {
                _nextTablePageButton.Visibility = ViewStates.Invisible;
            }
        }

        private int FindPlaceForNewDiagnotic(Diagnostic diagnostic)
        {
            int amountOfDiagnostics = _diagnostics.Count;
            int newDiagnosticIndex = amountOfDiagnostics;

            for (int i = 0; i < amountOfDiagnostics; ++i)
            {
                if (diagnostic.Date < _diagnostics[i].Date)
                {
                    newDiagnosticIndex = i;
                    break;
                }
            }

            return newDiagnosticIndex;
        }

        private void AdjustVisibilityOfConrolButtons()
        {
            if (_trip == null)
            {
                _addDiagnosticButton.Visibility = ViewStates.Gone;
                _addFirstDiagnosticButton.Visibility = ViewStates.Gone;
                FindViewById<TextView>(Resource.Id.textView2).Visibility = ViewStates.Gone;
            }
            else
            {
                _addDiagnosticButton.Visibility = ViewStates.Visible;
                _addFirstDiagnosticButton.Visibility = ViewStates.Visible;
                FindViewById<TextView>(Resource.Id.textView2).Visibility = ViewStates.Visible;
            }
        }

        private void AdjustVisibilityOfShowGraphButton()
        {
            if(_diagnostics.Count > 1)
            {
                _showGraphButton.Visibility = ViewStates.Visible;
            }
            else
            {
                _showGraphButton.Visibility = ViewStates.Gone;
            }
        }
        #endregion
    }
}