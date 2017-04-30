using System;
using System.Timers;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AlertDialog = Android.App.AlertDialog;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "LoginActivity", MainLauncher = true)]
    public class LoginActivity : ActionBarActivity
    {
        #region Constants
        private static readonly string DefaultPassword = "123";
        private static readonly int HidePasswordInfoTimer = 2000;
        #endregion

        #region Widgets
        private LinearLayout _defaultUser;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.log_in);

            /* Set tool bar */
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Window.AddFlags(WindowManagerFlags.Fullscreen);

            /* Set view handlers */
            _defaultUser = FindViewById<LinearLayout>(Resource.Id.li_user);
            _defaultUser.Click += OnUserClicked;
        }

        private void OnUserClicked(object s, EventArgs e)
        {
            using (var builder = new AlertDialog.Builder(this))
            {
                /* Create and show dialog */
                var dialogView = LayoutInflater.Inflate(Resource.Layout.password_dialog, null);
                builder.SetView(dialogView);
                builder.SetPositiveButton(GetString(Resource.String.log_in), (EventHandler<DialogClickEventArgs>)null);
                builder.SetNegativeButton(GetString(Resource.String.cancel_button), (EventHandler<DialogClickEventArgs>)null);
                var dialog = builder.Create();
                dialog.Show();

                /* Set button handlers */
                var positiveButton = dialog.GetButton((int) DialogButtonType.Positive);
                var negativeButton = dialog.GetButton((int)DialogButtonType.Negative);

                positiveButton.Click += (sender, args) =>
                {
                    var infoField = dialogView.FindViewById<TextView>(Resource.Id.password_info);
                    var password = dialogView.FindViewById<EditText>(Resource.Id.li_password).Text;

                    /* If password is not correct */
                    if (password != DefaultPassword)
                    {
                        infoField.Text = GetString(Resource.String.wrong_password);
                        infoField.Visibility = ViewStates.Visible;

                        /* Timer to hide password info */
                        var timer = new Timer(HidePasswordInfoTimer);
                        timer.Elapsed += (timerSender, timerArgs) =>
                        {
                            var timerToStop = timerSender as Timer;
                            timerToStop?.Stop();

                            RunOnUiThread(() =>
                            {
                                infoField.Visibility = ViewStates.Gone;
                                infoField.Text = "";
                            });
                        };
                        timer.Start();
                    }
                    else
                    {
                        // Call HomeScreenActivity
                        dialog.Dismiss();
                    }
                };

                negativeButton.Click += (sender, args) =>
                {
                    dialog.Dismiss();
                };
            }
        }
    }
}