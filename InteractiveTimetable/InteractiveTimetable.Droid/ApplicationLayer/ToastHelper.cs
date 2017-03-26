using Android.App;
using Android.Views;
using Android.Widget;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    public static class ToastHelper
    {
        public static Toast GetErrorToast(Activity activity, string errorMessage)
        {
            /* Getting layout */
            var inflater1 = activity.LayoutInflater;
            View layout = inflater1.Inflate(Resource.Layout.error_message,
                                            (ViewGroup) activity.FindViewById(Resource.Id.error_message));

            /* Setting error message */
            TextView text = layout.FindViewById<TextView>(Resource.Id.error_text);
            text.Text = errorMessage;

            /* Showing toast */
            Toast toast = new Toast(activity.ApplicationContext);
            toast.SetGravity(GravityFlags.ClipVertical, 0, 0);
            toast.Duration = ToastLength.Long;
            toast.View = layout;

            return toast;
        }
    }
}