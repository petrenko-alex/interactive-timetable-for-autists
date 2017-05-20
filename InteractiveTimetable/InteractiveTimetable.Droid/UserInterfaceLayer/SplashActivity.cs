using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.Droid.ApplicationLayer;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(
        Label = "Интерактивное расписание", 
        MainLauncher = true,  
        Theme = "@style/MyTheme.SplashTheme", 
        NoHistory = true
        )]
    class SplashActivity : AppCompatActivity
    {
        private static readonly int ActivityCardsCount = 24;
        private static readonly int GoalCardsCount = 5;
        private static readonly int CardImageSizeDp = 140;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AddDefaultCards();

            var intent = new Intent(this, typeof(LoginActivity));
            StartActivity(intent);
            Finish();
        }

        private async void AddDefaultCards()
        {
            /* Add default cards if no cards yet in db */
            bool noCards = InteractiveTimetable.Current.ScheduleManager.Cards.CardCount == 0;
            if (noCards)
            {
                var imageSize = ImageHelper.ConvertDpToPixels(CardImageSizeDp);

                /* Add activity cards */
                for (int i = 0; i < ActivityCardsCount; ++i)
                {
                    /* Get and save card image */
                    string cardName = "activity_card_" + (i + 1);
                    int id = Resources.GetIdentifier(cardName, "drawable", PackageName);
                    var cardBitmap = await id.LoadScaledDownBitmapForDisplayAsync(
                        Resources,
                        imageSize,
                        imageSize
                    );
                    var filePath = InteractiveTimetable.ExportBitmapAsPng(cardBitmap, cardName);

                    /* Save card in DB */
                    int cardTypeId = InteractiveTimetable.Current.ScheduleManager.Cards.CardTypes.
                                                          GetActivityCardType().Id;
                    var card = new Card()
                    {
                        CardTypeId = cardTypeId,
                        PhotoPath = filePath
                    };
                    InteractiveTimetable.Current.ScheduleManager.Cards.SaveCard(card);
                }

                /* Add motivation goal cards */
                for (int j = 0; j < GoalCardsCount; ++j)
                {
                    /* Get and save card image */
                    string cardName = "motivation_goal_card_" + (j + 1);
                    int id = Resources.GetIdentifier(cardName, "drawable", PackageName);
                    var cardBitmap = await id.LoadScaledDownBitmapForDisplayAsync(
                        Resources,
                        imageSize,
                        imageSize
                    );
                    var filePath = InteractiveTimetable.ExportBitmapAsPng(cardBitmap, cardName);

                    /* Save card in DB */
                    int cardTypeId = InteractiveTimetable.Current.ScheduleManager.Cards.CardTypes.
                                                          GetMotivationGoalCardType().Id;
                    var card = new Card()
                    {
                        CardTypeId = cardTypeId,
                        PhotoPath = filePath
                    };
                    InteractiveTimetable.Current.ScheduleManager.Cards.SaveCard(card);
                }
            }
        }

        public override void OnBackPressed() {}
    }
}