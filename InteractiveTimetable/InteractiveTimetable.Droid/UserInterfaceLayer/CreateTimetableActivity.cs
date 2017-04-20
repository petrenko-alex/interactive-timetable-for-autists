using System;
using System.Globalization;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Widget;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.Droid.ApplicationLayer;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "Create Timetable")]
    public class CreateTimetableActivity : Activity
    {
        #region Constants
        private static readonly int CardColumnWidth = 120;
        #endregion

        #region Widgets
        private TextView _label;
        private TextView _currentDateView;
        private ImageButton _backButton;
        private RecyclerView _goalCards;

        #region Activity Cards Widgets
        private RecyclerView _activityCards;
        private GridAutofitLayoutManager _activityCardsLayoutManager;
        private CardListAdapter _activityCardsAdapter;
        #endregion

        #endregion

        #region Internal Variables
        private User _currentUser;
        private DateTime _currentDate;
        private int _recyclerViewWidth;
        private int _recyclerViewItemWidth;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.create_timetable);

            /* Hide action bar */
            ActionBar.Hide();

            /* Get data */
            int userId = Intent.GetIntExtra("user_id", 0);
            string currentDate = Intent.GetStringExtra("date");
            _currentUser = InteractiveTimetable.Current.UserManager.GetUser(userId);
            _currentDate = DateTime.ParseExact(
                currentDate,
                "dd.MM.yyyy",
                CultureInfo.CurrentCulture
            );

            /* Get views */
            _label = FindViewById<TextView>(Resource.Id.ct_label);
            _currentDateView = FindViewById<TextView>(Resource.Id.ct_current_date);
            _backButton = FindViewById<ImageButton>(Resource.Id.ct_back_button);
            _activityCards = FindViewById<RecyclerView>(Resource.Id.ct_activity_cards);

            /* Set data for view */
            string label = $"{_currentUser.FirstName} {_currentUser.LastName} - {_label.Text}";
            _label.Text = label;
            _currentDateView.Text = _currentDate.ToString("dd MMMM yyyy");

            /* Set handlers */
            _backButton.Click += OnBackButtonClicked;

            /* Add cards */
            AddActivityCards();
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
        {
            SetResult(Result.Canceled, null);
            Finish();
        }

        private void OnAddActivityCardButtonClicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void AddActivityCards()
        {
            /* Get data */
            var cards = InteractiveTimetable.Current.ScheduleManager.Cards.
                                             GetActivityCards().
                                             ToList();
            /* Add empty card for add button */
            cards.Add(new Card()
            {
                Id = 0
            });

            /* Set up layout manager for activity cards recycler view */
            _activityCardsLayoutManager = new GridAutofitLayoutManager(this, CardColumnWidth);
            _activityCards.SetLayoutManager(_activityCardsLayoutManager);
            
            /* Set up the adapter for activity cards recycler view */
            _activityCardsAdapter = new CardListAdapter(this, cards);
            _activityCardsAdapter.ItemClick += OnActivityCardClick;
            _activityCardsAdapter.RequestToDeleteItem += OnRequestToDeleteActivityCard;
            _activityCards.SetAdapter(_activityCardsAdapter);
        }

        private void OnRequestToDeleteActivityCard(int cardId, int positionInList)
        {
            throw new NotImplementedException();
        }

        private void OnActivityCardClick(int cardId, ImageView cardImage)
        {
            throw new NotImplementedException();
        }

        //        private void OnCreateTimetableClicked(object sender, EventArgs e)
//        {
//            var intent = new Intent(this, typeof(TimetableActivity));
//            intent.PutExtra("timetable", int[]);
//            SetResult(Result.Ok, intent);
//            Finish();
//        }
    }
}