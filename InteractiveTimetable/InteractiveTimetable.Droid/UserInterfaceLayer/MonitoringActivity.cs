using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidViewAnimations;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.Droid.ApplicationLayer;
using AlertDialog = Android.App.AlertDialog;
using Object = Java.Lang.Object;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "MonitoringActivity")]
    public class MonitoringActivity : 
        ActionBarActivity, 
        ViewTreeObserver.IOnGlobalLayoutListener,
        IDiagnosticDialogListener
    {
        #region Constants
        internal static readonly int HeaderColumnWidth = 150;
        internal static readonly int HeaderColumnHeight = 50;
        internal static readonly int GradeColumnWidth = 50;
        internal static readonly int GradeColumnHeight = 50;
        internal static readonly string TickUnicodeSymbol = "\u2713";
        private static readonly int MaxVisibleDiagnostics = 3;
        #endregion

        #region Widgets
        internal LinearLayout LayoutForTable;
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
        internal HospitalTrip Trip;
        internal List<Diagnostic> Diagnostics;
        internal List<TableLayout> Tables;
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
            LayoutForTable = FindViewById<LinearLayout>(Resource.Id.table_layout);
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

            /* Set data to views */
            if (tripId > 0)
            {
                Trip = InteractiveTimetable.Current.HospitalTripManager.GetHospitalTrip(tripId);
                string text = $"{GetString(Resource.String.trip_in_list)}" +
                              $"{Trip.Number} " +
                              $"{Trip.StartDate:dd.MM.yyyy} - {Trip.FinishDate:dd.MM.yyyy}";
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
            Diagnostics = new List<Diagnostic>();
            _visibleDiagnosticIndexes = new List<int>();
            if (Trip != null)
            {
                Diagnostics = Trip.Diagnostics.OrderBy(x => x.Date).ToList();
            }
            else
            {
                var trips = _user.HospitalTrips;
                foreach (var trip in trips)
                {
                    Diagnostics.AddRange(trip.Diagnostics);
                }
                Diagnostics = Diagnostics.OrderBy(x => x.Date).ToList();
            }

            if (Diagnostics.Count > 0)
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
            new CreateGraphTask(this).Execute();
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

            var dialog = DiagnosticDialogFragment.NewInstance(0, Trip.Id);
            dialog.Show(transaction, DiagnosticDialogFragment.FragmentTag);
        }

        public void OnEditDiagnosticButtonClicked(int diagnosticId)
        {
            var transaction = FragmentManager.BeginTransaction();
            var previousFragment = FragmentManager.
                    FindFragmentByTag(DiagnosticDialogFragment.FragmentTag);

            if (previousFragment != null)
            {
                transaction.Remove(previousFragment);
            }
            transaction.AddToBackStack(null);

            var dialog = DiagnosticDialogFragment.NewInstance(diagnosticId, Trip.Id);
            dialog.Show(transaction, DiagnosticDialogFragment.FragmentTag);
        }

        public void OnDeleteDiagnosticButtonClicked(int diagnosticId)
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
            var diagnostic = Diagnostics.First(x => x.Id == diagnosticId);
            int index = Diagnostics.IndexOf(diagnostic);

            /* Delete from database */
            InteractiveTimetable.Current.DiagnosticManager.DeleteDiagnostic(diagnosticId);

            /* Delete from data sets */
            Diagnostics.RemoveAt(index);
            Tables.RemoveAt(index);

            /* Delete from layout */
            var table = LayoutForTable.GetChildAt(index + 1);
            LayoutForTable.RemoveView(table);

            int visibleAmount = _visibleDiagnosticIndexes.Count;
            int lastVisible = _visibleDiagnosticIndexes[visibleAmount - 1];
            if (lastVisible == Diagnostics.Count)
            {
                /* Create new visible list */
                _visibleDiagnosticIndexes.Clear();
                for (int i = Diagnostics.Count - 1, j = 0; i >= 0; --i, ++j)
                {
                    if (j < MaxVisibleDiagnostics)
                    {
                        InsertOrReplaceInVisibleList(j, i);
                    }
                }

                _visibleDiagnosticIndexes = _visibleDiagnosticIndexes.OrderBy(x => x).ToList();
            }

            if (Diagnostics.Count == 0)
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
                    var table = Tables[index];
                    
                    /* Remove old table from layout */
                    LayoutForTable.RemoveViewAt(i + 1);

                    if (table != null)
                    {
                        LayoutForTable.AddView(table, i + 1);
                    }
                    else
                    {
                        var task = new CreateDiagnosticTableTask(this, index, i + 1);
                        task.Execute(null, null);
                    }
                }

                AsjustVisibilityOfTablePageButtons();
            }
        }

        private void OnNextTablePageButtonClicked(object sender, EventArgs e)
        {
            /* Check if has diagnostics */
            int lastVisibleDiagnosticIndex = _visibleDiagnosticIndexes[MaxVisibleDiagnostics - 1];
            if (lastVisibleDiagnosticIndex != Diagnostics.Count - 1)
            {
                /* Increment indexes in _visibleDiagnosticIndexes */
                for (int i = 0; i < MaxVisibleDiagnostics; ++i)
                {
                    _visibleDiagnosticIndexes[i]++;
                }

                /* Delete current visible tables from layout */
                for (int i = MaxVisibleDiagnostics; i > 0; --i)
                {
                    var table = LayoutForTable.GetChildAt(i);
                    LayoutForTable.RemoveView(table);
                }

                /* Set tables */
                for (int i = 0; i < MaxVisibleDiagnostics; ++i)
                {
                    /* If already has table for diagnostic */
                    int index = _visibleDiagnosticIndexes[i];
                    var table = Tables[index];

                    if (table != null)
                    {
                        /* Set another table to layout */
                        LayoutForTable.AddView(table, i + 1);
                    }
                    else
                    {
                        /* Need to create new table */
                        var task = new CreateDiagnosticTableTask(this, index, i + 1);
                        task.Execute(null, null);
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
            LayoutForTable.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);

            /* Adjust table controls width to match table width */
            var width = LayoutForTable.Width;
            var tableControls = FindViewById<RelativeLayout>(Resource.Id.table_controls);
            tableControls.LayoutParameters.Width = width;
        }

        public void OnNewDiagnosticAdded(int diagnosticId)
        {
            if (Diagnostics.Count == 0)
            {
                AddFirstDiagnostic();
                return;
            }

            /* Get just added diagnostic */
            var diagnostic = InteractiveTimetable.Current.DiagnosticManager.
                                                  GetDiagnostic(diagnosticId);

            int newDiagnosticIndex = FindPlaceForNewDiagnotic(diagnostic);

            /* Insert new diagnostic in _diagnostics data set */
            Diagnostics.Insert(newDiagnosticIndex, diagnostic);

            /* Insert null table to _tables data set */
            Tables.Insert(newDiagnosticIndex, null);

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

            var previousDiagnosticData = Diagnostics.First(x => x.Id == diagnostic.Id);
            bool datesEqual = diagnostic.Date.Equals(previousDiagnosticData.Date);
            if (!datesEqual)
            {
                /* Delete old diagnostic data from data sets */
                int index = Diagnostics.IndexOf(previousDiagnosticData);
                Diagnostics.RemoveAt(index);
                Tables.RemoveAt(index);

                /* Insert in new position */
                int newDiagnosticIndex = FindPlaceForNewDiagnotic(diagnostic);
                Diagnostics.Insert(newDiagnosticIndex, diagnostic);
                Tables.Insert(newDiagnosticIndex,null);

                /* Rebuild _visibleDiagnosticIndexes */
                RebuildVisibleList(newDiagnosticIndex);

                RebuildVisibleTables();

                AsjustVisibilityOfTablePageButtons();
            }
            else
            {
                /* If date of the diagnostic was not changed */
                int index = Diagnostics.IndexOf(previousDiagnosticData);

                /* Update data sets */
                Diagnostics[index] = diagnostic;
                Tables[index] = null;

                RebuildVisibleTables();
            }
        }

        private void AdjustVisibilityOfNoDiagnosticsInfo()
        {
            if (Diagnostics.Count == 0)
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
            Trip = InteractiveTimetable.Current.HospitalTripManager.GetHospitalTrip(Trip.Id);
            Diagnostics = Trip.Diagnostics.OrderBy(x => x.Date).ToList();
            CreateTables();
            AdjustVisibilityOfNoDiagnosticsInfo();
        }
    
        private void CreateTables()
        {
            /* If not created yet */
            if (LayoutForTable.ChildCount == 0)
            {
                /* Create header table */
                AddHeaderTable();
            }

            /* First initialize with null's to fit _diagnostics size */
            int diagnosticsAmount = Diagnostics.Count;
            Tables = new List<TableLayout>();
            for (int i = 0; i < diagnosticsAmount; ++i)
            {
                Tables.Add(null);
            }

            /* Create tables for diagnostics */
            if (diagnosticsAmount > MaxVisibleDiagnostics)
            {
                diagnosticsAmount = MaxVisibleDiagnostics;
            }
            for (int i = 0; i < diagnosticsAmount; ++i)
            {
                var task = new CreateDiagnosticTableTask(this, i);
                task.Execute(null, null);
                _visibleDiagnosticIndexes.Add(i);
            }

            /* Adjust table page control buttons */
            _previousTablePageButton.Visibility = ViewStates.Invisible;

            if (Diagnostics.Count <= MaxVisibleDiagnostics)
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

            if (Trip != null)
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
            LayoutForTable.AddView(_headerTable);
        }

        public TableLayout CreateTable(LinearLayout.LayoutParams layoutParams = null)
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

        public TableRow CreateRow()
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

        public TextView CreateColumn(TableRow.LayoutParams layoutParams, string columnText)
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

        public void SetColumnColor(int grade, TextView column)
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
                var table = LayoutForTable.GetChildAt(i);
                LayoutForTable.RemoveView(table);
            }

            /* Set new tables */
            for (int i = 0; i < amountOfVisible; ++i)
            {
                /* If already has table for diagnostic */
                int index = _visibleDiagnosticIndexes[i];
                var table = Tables[index];
                if (table != null)
                {
                    LayoutForTable.AddView(table, i + 1);
                }
                else
                {
                    var task = new CreateDiagnosticTableTask(this, index, i + 1);
                    task.Execute(null, null);
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
                newDiagnosticIndex < (Diagnostics.Count - 1))
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
                       i < Diagnostics.Count)
                {
                    InsertOrReplaceInVisibleList(i, i);
                    i++;
                }
            }
            /* If insert as last */
            else if (newDiagnosticIndex == (Diagnostics.Count - 1))
            {
                if (Diagnostics.Count >= MaxVisibleDiagnostics)
                {
                    InsertOrReplaceInVisibleList(2, newDiagnosticIndex);
                    InsertOrReplaceInVisibleList(1, newDiagnosticIndex - 1);
                    InsertOrReplaceInVisibleList(0, newDiagnosticIndex - 2);
                }
                else if (Diagnostics.Count == MaxVisibleDiagnostics - 1)
                {
                    InsertOrReplaceInVisibleList(0, newDiagnosticIndex - 1);
                    InsertOrReplaceInVisibleList(1, newDiagnosticIndex);
                }
            }
        }

        public void AsjustVisibilityOfTablePageButtons()
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
            if (lastVisible < (Diagnostics.Count - 1))
            {
                _nextTablePageButton.Visibility = ViewStates.Visible;
            }
            else if (lastVisible == (Diagnostics.Count - 1))
            {
                _nextTablePageButton.Visibility = ViewStates.Invisible;
            }
        }

        private int FindPlaceForNewDiagnotic(Diagnostic diagnostic)
        {
            int amountOfDiagnostics = Diagnostics.Count;
            int newDiagnosticIndex = amountOfDiagnostics;

            for (int i = 0; i < amountOfDiagnostics; ++i)
            {
                if (diagnostic.Date < Diagnostics[i].Date)
                {
                    newDiagnosticIndex = i;
                    break;
                }
            }

            return newDiagnosticIndex;
        }

        private void AdjustVisibilityOfConrolButtons()
        {
            if (Trip == null)
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
            if(Diagnostics.Count > 1)
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

    internal class CreateDiagnosticTableTask : AsyncTask
    {
        private ProgressDialog _progress;
        private readonly MonitoringActivity _parent;
        private readonly int _diagnosticNumber;
        private readonly int _insertTableAt;

        public CreateDiagnosticTableTask(
            MonitoringActivity parent, 
            int diagnosticNumber, 
            int insertTableAt = -1)
        {
            _parent = parent;
            _diagnosticNumber = diagnosticNumber;
            _insertTableAt = insertTableAt;
        }

        protected override void OnPostExecute(Object result)
        {
            var table = (TableLayout) result;

            /* Add table to data set */
            _parent.Tables[_diagnosticNumber] = table;

            /* Add table to layout */
            if (_insertTableAt >= 0)
            {
                _parent.LayoutForTable.AddView(table, _insertTableAt);
            }
            else
            {
                _parent.LayoutForTable.AddView(table);
            }
            
            /* Animate table */
            YoYo.With(Techniques.FadeIn).Duration(700).PlayOn(table);

            /* Adjust control buttons */
            _parent.LayoutForTable.ViewTreeObserver.AddOnGlobalLayoutListener(_parent);
            _parent.AsjustVisibilityOfTablePageButtons();

            /* Close loading dialog */
            _progress.Dismiss();
        }

        protected override void OnPreExecute()
        {
            /* Show loading dialog */
            _progress = new ProgressDialog(_parent);
            _progress.SetMessage(_parent.GetString(Resource.String.loading));
            _progress.SetCancelable(false); 
            _progress.Show();
        }

        protected override Object DoInBackground(params Object[] @params)
        {
            /* Prepare data */
            var gradesAmount = 4;
            var diagnostic = _parent.Diagnostics[_diagnosticNumber];

            var paramsForTable = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent
            );
            int marginDp = 10;
            int marginPx = ImageHelper.ConvertDpToPixels(marginDp);
            paramsForTable.SetMargins(marginPx, 0, marginDp, 0);

            var paramsForDiagnostic = new TableRow.LayoutParams(
                MonitoringActivity.GradeColumnWidth,
                MonitoringActivity.GradeColumnHeight
            );

            var paramsForDate = new TableRow.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                MonitoringActivity.GradeColumnHeight
            )
            {
                Span = 4
            };

            /* Create table */
            var table = _parent.CreateTable(paramsForTable);
            table.SetBackgroundResource(Resource.Drawable.table_frame);
            var paddingDp = 4;
            var paddingPx = ImageHelper.ConvertDpToPixels(paddingDp);
            table.SetPadding(paddingDp, paddingPx, paddingPx, paddingPx);

            /* Create row with manage buttons */
            if (_parent.Trip != null)
            {
                var paramsForManagement = new TableRow.LayoutParams(
                    ViewGroup.LayoutParams.WrapContent,
                    MonitoringActivity.GradeColumnHeight
                )
                {
                    Span = 4,
                    Gravity = GravityFlags.Center
                };

                var view = LayoutInflater.From(_parent).Inflate(Resource.Layout.management_column, null);
                view.LayoutParameters = paramsForManagement;

                var editButton = view.FindViewById<ImageButton>(
                    Resource.Id.edit_diagnostic_button
                );
                var deleteButton = view.FindViewById<ImageButton>(
                    Resource.Id.delete_diagnostic_button
                );

                editButton.Click += (sender, e) => _parent.OnEditDiagnosticButtonClicked(diagnostic.Id);
                deleteButton.Click += (sender, e) => _parent.OnDeleteDiagnosticButtonClicked(diagnostic.Id);

                /* Add to table */
                var manageRow = _parent.CreateRow();
                manageRow.AddView(view);
                table.AddView(manageRow);
            }

            /* Create grade header column */
            var firstRow = _parent.CreateRow();
            for (int i = 0; i < gradesAmount; ++i)
            {
                var gradeHeaderColumn = _parent.CreateColumn(paramsForDiagnostic, $"{i + 1}");
                _parent.SetColumnColor(i + 1, gradeHeaderColumn);
                firstRow.AddView(gradeHeaderColumn);
            }
            table.AddView(firstRow);

            /* Create date column */
            var secondRow = _parent.CreateRow();
            var dateColumn = _parent.CreateColumn(
                paramsForDate,
                diagnostic.Date.ToString("dd.MM.yyyy H:mm")
            );
            secondRow.AddView(dateColumn);
            table.AddView(secondRow);

            /* Create grade columns */
            var paramsForGrade = new TableRow.LayoutParams(
                MonitoringActivity.GradeColumnWidth,
                MonitoringActivity.GradeColumnHeight
            );

            foreach (var grade in diagnostic.CriterionGrades)
            {
                /* Create 4 columns for grades */
                var currentRow = _parent.CreateRow();
                for (int i = 0; i < gradesAmount; ++i)
                {
                    var gradeColumn = _parent.CreateColumn(paramsForGrade, "");
                    currentRow.AddView(gradeColumn);
                }

                if (grade.Grade == "1")
                {
                    var column = (TextView)currentRow.GetChildAt(0);
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
                            column.Text = MonitoringActivity.TickUnicodeSymbol;
                        }
                    }
                }

                table.AddView(currentRow);
            }

            /* Set partial sums */
            var partialSumRow = _parent.CreateRow();
            for (int i = 0; i < gradesAmount; ++i)
            {
                int sum = InteractiveTimetable.Current.DiagnosticManager.
                                               GetPartialSum(diagnostic, i + 1);
                var sumColumn = _parent.CreateColumn(paramsForGrade, sum + "");
                partialSumRow.AddView(sumColumn);
            }
            table.AddView(partialSumRow);

            /* Set total sum */
            var totalSumRow = _parent.CreateRow();
            int totalSum = InteractiveTimetable.Current.DiagnosticManager.GetTotalSum(diagnostic);
            var totalSumColumn = _parent.CreateColumn(paramsForDate, totalSum + "");
            totalSumRow.AddView(totalSumColumn);
            table.AddView(totalSumRow);

            return table;
        }
    }

    internal class CreateGraphTask : AsyncTask
    {
        private ProgressDialog _progress;
        private readonly MonitoringActivity _parent;
        private readonly List<int> _results;
        private List<DateTime> _dates;

        public CreateGraphTask(MonitoringActivity parent)
        {
            _parent = parent;
            _dates = new List<DateTime>();
            _results = new List<int>();
        }

        protected override void OnPostExecute(Object result)
        {
            /* Stop loading dialog */
            _progress.Dismiss();

            var transaction = _parent.FragmentManager.BeginTransaction();
            var previousFragment = _parent.FragmentManager.FindFragmentByTag(
                DiagnosticDialogFragment.FragmentTag
            );

            if (previousFragment != null)
            {
                transaction.Remove(previousFragment);
            }
            transaction.AddToBackStack(null);

            /* Create and show graph dialog */
            var dialog = GraphDialogFragment.NewInstance(_results, _dates);
            dialog.Show(transaction, GraphDialogFragment.FragmentTag);
        }

        protected override void OnPreExecute()
        {
            /* Show loading dialog */
            _progress = new ProgressDialog(_parent);
            _progress.SetMessage(_parent.GetString(Resource.String.loading));
            _progress.SetCancelable(false);
            _progress.Show();
        }

        protected override Object DoInBackground(params Object[] @params)
        {
            /* Prepare data for graph */
            foreach (var diagnostic in _parent.Diagnostics)
            {
                var sum = InteractiveTimetable.Current.DiagnosticManager.GetTotalSum(diagnostic);
                _results.Add(sum);
            }

            _dates = _parent.Diagnostics.Select(x => x.Date).ToList();

            return null;
        }
    }
}