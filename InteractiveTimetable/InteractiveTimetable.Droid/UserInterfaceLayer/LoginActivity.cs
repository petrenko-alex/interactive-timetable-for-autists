using System;
using System.Timers;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidViewAnimations;
using AlertDialog = Android.App.AlertDialog;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    [Activity(Label = "Вход в систему", MainLauncher = true)]
    public class LoginActivity : ActionBarActivity
    {
        #region Constants
        private static readonly string DefaultPassword = "123";
        private static readonly int HidePasswordInfoTimer = 2000;
        private static readonly int WrongPasswordAnimationDuration = 700;
        private static readonly int HomeScreenRequest = 1;
        #endregion

        #region Widgets
        private LinearLayout _defaultUser;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.log_in);

            /* Set tool bar */
            var toolbar = FindViewById<Toolbar>(Resource.Id.li_toolbar);
            SetSupportActionBar(toolbar);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            AdjustToolbarForActivity();

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
                    var passwordField = dialogView.FindViewById<EditText>(Resource.Id.li_password);
                    var password = passwordField.Text;

                    /* If password is not correct */
                    if (password != DefaultPassword)
                    {
                        infoField.Text = GetString(Resource.String.wrong_password);
                        infoField.Visibility = ViewStates.Visible;

                        /* Show animation */
                        YoYo.With(Techniques.Bounce).
                             Duration(WrongPasswordAnimationDuration).
                             PlayOn(passwordField);

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
                        /* Call home screen activity */
                        var intent = new Intent(this, typeof(HomeScreenActivity));
                        intent.PutExtra("user_name", "Светлана");
                        StartActivityForResult(intent, HomeScreenRequest);

                        dialog.Dismiss();
                    }
                };

                negativeButton.Click += (sender, args) =>
                {
                    dialog.Dismiss();
                };
            }
        }

        private void AdjustToolbarForActivity()
        {
            /* Set toolbar layout */
            var toolbar = FindViewById<Toolbar>(Resource.Id.li_toolbar);
            var toolbarContent = FindViewById<LinearLayout>(Resource.Id.toolbar_content);
            var layout = LayoutInflater.Inflate(Resource.Layout.login_toolbar, toolbar, false);
            toolbarContent.AddView(layout);

            /* Set toolbar controls */
            var title = toolbar.FindViewById<TextView>(Resource.Id.toolbar_title);
            title.Text = GetString(Resource.String.logging_in);

            var clock = toolbar.FindViewById<TextClock>(Resource.Id.toolbar_clock);
            clock.Format24Hour = InteractiveTimetable.DateTimeFormat;
        }
    }
}