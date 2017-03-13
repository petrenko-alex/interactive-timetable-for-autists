using Android.App;
using Android.OS;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "Management", MainLauncher = true)]
    public class ManagementActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

			SetContentView (Resource.Layout.Management);
        }
    }
}