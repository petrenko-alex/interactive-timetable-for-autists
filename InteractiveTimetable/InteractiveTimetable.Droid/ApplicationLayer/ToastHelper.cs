using Android.App;
using Android.Views;
using Android.Widget;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    public static class ToastHelper
    {
        public static Toast GetErrorToast(Activity activity, string errorMessage)
        {
            /* Get layout */
            var inflater = activity.LayoutInflater;
            var layout = inflater.Inflate(Resource.Layout.error_message,
                                          (ViewGroup)
                                          activity.FindViewById(Resource.Id.error_message));

            /* Set error message */
            var text = layout.FindViewById<TextView>(Resource.Id.error_text);
            text.Text = errorMessage;

            /* Show toast */
            var toast = new Toast(activity.ApplicationContext);
            toast.SetGravity(GravityFlags.ClipVertical, 0, 0);
            toast.Duration = ToastLength.Long;
            toast.View = layout;

            return toast;
        }

        public static Toast GetInfoToast(Activity activity, string infoMessage)
        {
            /* Get layout */
            var inflater = activity.LayoutInflater;
            var layout = inflater.Inflate(Resource.Layout.info_message,
                                          (ViewGroup)
                                          activity.FindViewById(Resource.Id.info_message));

            /* Set error message */
            var text = layout.FindViewById<TextView>(Resource.Id.info_text);
            text.Text = infoMessage;

            /* Show toast */
            var toast = new Toast(activity.ApplicationContext);
            toast.SetGravity(GravityFlags.ClipVertical, 0, 0);
            toast.Duration = ToastLength.Short;
            toast.View = layout;

            return toast;
        }
    }
}