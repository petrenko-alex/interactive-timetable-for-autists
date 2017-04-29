using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.Droid.ApplicationLayer;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    public class UserDetailsFragment : Fragment
    {
        #region Constants
        public static readonly string FragmentTag = "user_details_fragment";
        private static readonly string UserIdKey = "current_user_id";
        private static readonly int UserImageSizeDp = 300;
        #endregion

        #region Widgets
        private ImageButton _editButton;
        #endregion

        #region Events
        public event Action<int> EditButtonClicked;
        #endregion

        #region Properties
        public int UserId
        {
            get { return Arguments.GetInt(UserIdKey, 0); }
        }
        #endregion

        #region Methods

        #region Construct Methods
        public static UserDetailsFragment NewInstance(int userId)
        {
            var userDetailsFragment = new UserDetailsFragment()
            {
                Arguments = new Bundle()
            };
            userDetailsFragment.Arguments.PutInt(UserIdKey, userId);

            return userDetailsFragment;
        }
        #endregion

        #region Event Handlers
        public override void OnDestroy()
        {
            _editButton.Click -= OnEditUserButtonClicked;
            GC.Collect();

            base.OnDestroy();
        }

        public override View OnCreateView(
            LayoutInflater inflater,
            ViewGroup container,
            Bundle savedInstanceState)
        {
            if (container == null)
            {
                return null;
            }

            /* Getting data */
            var user = InteractiveTimetable.Current.UserManager.GetUser(UserId);
            if (user == null)
            {
                return null;
            }

            var userView = inflater.Inflate(Resource.Layout.user_details, container, false);

            /* Setting last name */
            var lastNameView = userView.FindViewById<TextView>(Resource.Id.user_details_last_name);
            lastNameView.Text += " " + user.LastName;

            /* Setting first name */
            var firstNameView = userView.FindViewById<TextView>(Resource.Id.user_details_first_name);
            firstNameView.Text += " " + user.FirstName;

            /* Setting patronymic name */
            var patronymicNameView = userView.FindViewById<TextView>(Resource.Id.user_details_patronymic_name);
            patronymicNameView.Text += " " + user.PatronymicName;

            /* Setting age */
            var ageView = userView.FindViewById<TextView>(Resource.Id.user_details_age);
            ageView.Text += " " + user.Age;

            /* Setting birth date */
            var birthView = userView.FindViewById<TextView>(Resource.Id.user_details_birth);
            birthView.Text += " " + user.BirthDate.ToString("dd.MM.yyyy");

            /* Setting photo */
            var photoView = userView.FindViewById<ImageView>(Resource.Id.user_details_photo);
            var imageSize = ImageHelper.ConvertDpToPixels(UserImageSizeDp);
            var bitmap = user.PhotoPath.LoadAndResizeBitmap(imageSize, imageSize);
            if (bitmap != null)
            {
                photoView.SetImageBitmap(bitmap);
            }

            /* Setting button click handlers */
            _editButton = userView.FindViewById<ImageButton>(Resource.Id.edit_user);
            _editButton.Click += OnEditUserButtonClicked;

            return userView;
        }
    
        private void OnEditUserButtonClicked(object sender, EventArgs args)
        {
            EditButtonClicked?.Invoke(UserId);
        }
        #endregion

        #endregion
    }
}