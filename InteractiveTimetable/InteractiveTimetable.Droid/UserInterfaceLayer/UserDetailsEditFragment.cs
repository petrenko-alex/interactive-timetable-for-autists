using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.Droid.ApplicationLayer;
using Android.Graphics;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    public class UserDetailsEditFragment : Fragment
    {
        public static readonly string FragmentTag = "user_details_edit_fragment";
        private static readonly string UserIdKey = "current_user_id";

        private Button _applyButton;
        private Button _cancelButton;
        private ImageButton _datePickButton;

        public int UserId
        {
            get { return Arguments.GetInt(UserIdKey, 0); }
        }

        public static UserDetailsEditFragment NewInstance(int userId)
        {
            var userDetailsEditFragment = new UserDetailsEditFragment()
            {
                Arguments = new Bundle()
            };
            userDetailsEditFragment.Arguments.PutInt(UserIdKey, userId);

            return userDetailsEditFragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if(container == null)
            {
                return null;
            }

            View userView = inflater.Inflate(Resource.Layout.user_details_edit, container, false);

            /* Setting button click handlers */
            _applyButton = userView.FindViewById<Button>(Resource.Id.apply_changes_btn);
            _applyButton.Click += OnApplyButtonClicked;

            _cancelButton = userView.FindViewById<Button>(Resource.Id.cancel_btn);
            _cancelButton.Click += OnCancelButtonClicked;

            _datePickButton = userView.FindViewById<ImageButton>(Resource.Id.birth_date_edit);
            _datePickButton.Click += OnDatePickButtonClicked;

            /* If user is set, retrieve his data */
            if (UserId > 0)
            {
                /* Getting data */
                var user = InteractiveTimetable.Current.UserManager.GetUser(UserId);

                /* Setting last name */
                var lastNameView = userView.FindViewById<EditText>(Resource.Id.last_name_edit);
                lastNameView.Text = user.LastName;

                /* Setting first name */
                var firstNameView = userView.FindViewById<EditText>(Resource.Id.first_name_edit);
                firstNameView.Text = user.FirstName;

                /* Setting patronymic name */
                var patronymicNameView = userView.FindViewById<EditText>(Resource.Id.patronymic_name_edit);
                patronymicNameView.Text = user.PatronymicName;

                /* Setting birth date */
                var birthDateView = userView.FindViewById<EditText>(Resource.Id.birth_date_show);
                birthDateView.Text = user.BirthDate.ToString("dd.MM.yyyy");

                /* Setting photo */
                var photoView = userView.FindViewById<ImageView>(Resource.Id.user_details_photo);
                photoView.SetImageURI(Android.Net.Uri.Parse(user.PhotoPath));
                photoView.SetScaleType(ImageView.ScaleType.CenterCrop);
                photoView.SetPadding(0, 0, 0, 0);

                /* Setting frame */
                var frame = userView.FindViewById<FrameLayout>(Resource.Id.user_details_photo_frame);

                int paddingForFrameInDp = 1;
                int paddingForFrameInPixels = ImageHelper.
                    ConvertDpToPixels(paddingForFrameInDp, InteractiveTimetable.Current.ScreenDensity);

                frame.SetPadding(
                    paddingForFrameInPixels, 
                    paddingForFrameInPixels, 
                    paddingForFrameInPixels, 
                    paddingForFrameInPixels);
                frame.SetBackgroundColor(Color.ParseColor(ImageHelper.HexFrameColor));

                /* Adjust apply button */
                _applyButton.Text = GetString(Resource.String.edit_button);
            }

            return userView;
        }

        public override void OnDestroy()
        {
            _applyButton.Click -= OnApplyButtonClicked;
            _cancelButton.Click -= OnCancelButtonClicked;
            _datePickButton.Click -= OnDatePickButtonClicked;

            base.OnDestroy();
        }


        private void OnApplyButtonClicked(object sender, EventArgs args)
        {
            Console.WriteLine("Apply");

        }

        private void OnCancelButtonClicked(object sender, EventArgs args)
        {
            Console.WriteLine("Cancel");

        }

        private void OnDatePickButtonClicked(object sender, EventArgs args)
        {
            Console.WriteLine("Date");
        }
    }
}