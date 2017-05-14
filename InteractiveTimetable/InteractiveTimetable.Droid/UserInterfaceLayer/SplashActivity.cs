using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;

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
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var intent = new Intent(this, typeof(LoginActivity));
            StartActivity(intent);
            Finish();
        }

        public override void OnBackPressed() {}
    }
}