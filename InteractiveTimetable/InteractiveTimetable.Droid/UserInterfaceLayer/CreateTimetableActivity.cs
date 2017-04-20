using System;
using System.Globalization;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Widget;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.Droid.ApplicationLayer;
using Java.IO;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "Create Timetable")]
    public class CreateTimetableActivity : Activity
    {
        #region Constants
        private static readonly int CardColumnWidth = 120;
        private static readonly int ActivityCardViaCamera = 0;
        private static readonly int ActivityCardViaFile = 1;
        private static readonly int GoalCardViaCamera = 2;
        private static readonly int GoalCardViaFile = 3;
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
        private File _photo;
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
            _activityCardsAdapter.CardSelected += OnActivityCardClick;
            _activityCardsAdapter.RequestToDeleteItem += OnRequestToDeleteActivityCard;
            _activityCardsAdapter.AddCardButtonClicked += OnAddCardButtonClicked;
            _activityCards.SetAdapter(_activityCardsAdapter);
        }

        private void OnAddCardButtonClicked(int cardTypeId)
        {
            if (InteractiveTimetable.Current.HasCamera)
            {
                ChooseCardIfHasCamera(cardTypeId);
            }
            else
            {
                ChooseCardIfNoCamera(cardTypeId);
            }
        }

        private void OnRequestToDeleteActivityCard(int cardId, int positionInList)
        {
            throw new NotImplementedException();
        }

        private void OnActivityCardClick(int cardId, ImageView cardImage)
        {
            throw new NotImplementedException();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            /* If user chose photo */
            if (resultCode == Result.Ok)
            {
                string photoPath = "";
                
                /* Get path to image */
                if (requestCode == ActivityCardViaCamera || requestCode == GoalCardViaCamera)
                {
                    /* Make photo available in the gallery */
                    Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                    var contentUri = Android.Net.Uri.FromFile(_photo);
                    mediaScanIntent.SetData(contentUri);
                    SendBroadcast(mediaScanIntent);

                    photoPath = _photo.Path;

                }
                else if (requestCode == ActivityCardViaFile || requestCode == GoalCardViaFile)
                {
                    photoPath = InteractiveTimetable.Current.GetPathToImage(this, data.Data);
                }

                /* Create and add new card */
                /* Choose Card Type depending on request code */
                int cardTypeId = 0;
                if (requestCode == ActivityCardViaCamera ||
                    requestCode == ActivityCardViaFile)
                {
                    cardTypeId = InteractiveTimetable.Current.ScheduleManager.Cards.CardTypes.
                                                      GetActivityCardType().Id;
                }
                else if (requestCode == GoalCardViaCamera ||
                    requestCode == GoalCardViaFile)
                {
                    cardTypeId = InteractiveTimetable.Current.ScheduleManager.Cards.CardTypes.
                                                      GetMotivationGoalCardType().Id;
                }

                var newCard = new Card()
                {
                    CardTypeId = cardTypeId,
                    PhotoPath = photoPath
                };
                var cardId = InteractiveTimetable.Current.ScheduleManager.Cards.SaveCard(newCard);

                /* Add card to adapter */
                bool isActivityCard = InteractiveTimetable.Current.ScheduleManager.Cards.CardTypes.
                                                          IsActivityCardType(cardTypeId);
                if (isActivityCard)
                {
                    _activityCardsAdapter.InsertItem(cardId);
                }
                else
                {
                    //_goalCardsAdapter.InsertItem(cardId);
                }
            }
        }

        private void ChooseCardIfHasCamera(int cardTypeId)
        {
            /* Choose request code */
            int requestCode = InteractiveTimetable.Current.ScheduleManager.Cards.CardTypes.
                                                   IsActivityCardType(cardTypeId)
                ? ActivityCardViaCamera
                : GoalCardViaCamera;

            /* Prepare dialog items */
            string[] items =
            {
                GetString(Resource.String.take_a_photo),
                GetString(Resource.String.choose_from_gallery),
                GetString(Resource.String.cancel_button)
            };

            /* Construct dialog */
            using (var dialogBuilder = new AlertDialog.Builder(this))
            {
                dialogBuilder.SetTitle(GetString(Resource.String.add_card));

                dialogBuilder.SetItems(items, (d, args) => {

                    /* Taking a photo */
                    if (args.Which == 0)
                    {
                        var intent = new Intent(MediaStore.ActionImageCapture);

                        _photo = new File(InteractiveTimetable.Current.PhotoDirectory,
                                            $"card_{Guid.NewGuid()}.jpg");

                        intent.PutExtra(
                            MediaStore.ExtraOutput,
                            Android.Net.Uri.FromFile(_photo));

                        StartActivityForResult(intent, requestCode);
                    }
                    /* Choosing from gallery */
                    else if (args.Which == 1)
                    {
                        ChooseCardIfNoCamera(cardTypeId);
                    }
                });

                dialogBuilder.Show();
            }
        }

        private void ChooseCardIfNoCamera(int cardTypeId)
        {
            /* Choose request code */
            int requestCode = InteractiveTimetable.Current.ScheduleManager.Cards.CardTypes.
                                                   IsActivityCardType(cardTypeId)
                ? ActivityCardViaFile
                : GoalCardViaFile;

            var intent = new Intent(
                Intent.ActionPick,
                MediaStore.Images.Media.ExternalContentUri
            );

            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);

            StartActivityForResult(
                Intent.CreateChooser(
                    intent,
                    GetString(Resource.String.choose_photo)),
                requestCode
            );
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