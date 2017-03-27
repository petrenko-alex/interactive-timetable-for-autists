using System;
using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.Droid.ApplicationLayer;
using Android.Graphics;
using Android.Provider;
using InteractiveTimetable.BusinessLayer.Models;
using Java.IO;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    public class UserDetailsEditFragment : Fragment
    {
        public static readonly string FragmentTag = "user_details_edit_fragment";
        private static readonly string UserIdKey = "current_user_id";
        private static readonly int RequestCamera = 0;
        private static readonly int SelectFile = 1;

        #region Widgets

        private Button _applyButton;
        private Button _cancelButton;
        private Button _editPhotoButton;
        private ImageButton _datePickButton;
        private EditText _showDateField;
        private ImageView _userPhoto;
        private EditText _lastName;
        private EditText _firstName;
        private EditText _patronymicName;
        private EditText _birthDate;

        #endregion

        private User _user;
        private DateTime _currentDate;
        private File _photo;
        private Bitmap _bitmap;
        private Android.Net.Uri _currentUri;

        #region Flags

        private bool _fromGallery;
        private bool _dataWasChanged;
        private bool _photoWasChanged;

        #endregion


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

        public override View OnCreateView(
            LayoutInflater inflater, 
            ViewGroup container, 
            Bundle savedInstanceState)
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

            _editPhotoButton = userView.FindViewById<Button>(Resource.Id.edit_photo_btn);
            _editPhotoButton.Click += OnEditPhotoButtonClicked;

            _showDateField = userView.FindViewById<EditText>(Resource.Id.birth_date_show);
            _userPhoto = userView.FindViewById<ImageView>(Resource.Id.user_details_photo);

            /* If user is set, retrieve his data */
            if (UserId > 0)
            {
                /* Getting data */
                _user = InteractiveTimetable.Current.UserManager.GetUser(UserId);

                /* Setting last name */
                _lastName = userView.FindViewById<EditText>(Resource.Id.last_name_edit);
                _lastName.Text = _user.LastName;

                /* Setting first name */
                _firstName = userView.FindViewById<EditText>(Resource.Id.first_name_edit);
                _firstName.Text = _user.FirstName;

                /* Setting patronymic name */
                _patronymicName = userView.FindViewById<EditText>(Resource.Id.patronymic_name_edit);
                _patronymicName.Text = _user.PatronymicName;

                /* Setting birth date */
                _birthDate = userView.FindViewById<EditText>(Resource.Id.birth_date_show);
                _birthDate.Text = _user.BirthDate.ToString("dd.MM.yyyy");

                /* Setting photo */
                _userPhoto = userView.FindViewById<ImageView>(Resource.Id.user_details_photo);
                _userPhoto.SetImageURI(Android.Net.Uri.Parse(_user.PhotoPath));
                _userPhoto.SetScaleType(ImageView.ScaleType.CenterCrop);
                _userPhoto.SetPadding(0, 0, 0, 0);

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

                /* Adjust photo button */
                _editPhotoButton.Text = GetString(Resource.String.change_photo);

                /* Setting current date */
                _currentDate = _user.BirthDate;
            }

            /* Setting listeners for fields to track data changing */
            _firstName.TextChanged += OnUserDataChanged;
            _lastName.TextChanged += OnUserDataChanged;
            _patronymicName.TextChanged += OnUserDataChanged;
            _birthDate.TextChanged += OnUserDataChanged;

            /* Setting flags */
            _dataWasChanged = false;
            _photoWasChanged = false;

            return userView;
        }

        public override void OnDestroy()
        {
            /* Reseting listeners */
            _applyButton.Click -= OnApplyButtonClicked;
            _cancelButton.Click -= OnCancelButtonClicked;
            _datePickButton.Click -= OnDatePickButtonClicked;
            _editPhotoButton.Click -= OnEditPhotoButtonClicked;
            _firstName.TextChanged -= OnUserDataChanged;
            _lastName.TextChanged -= OnUserDataChanged;
            _patronymicName.TextChanged -= OnUserDataChanged;
            _birthDate.TextChanged -= OnUserDataChanged;

            GC.Collect();

            base.OnDestroy();
        }

        private void OnApplyButtonClicked(object sender, EventArgs args)
        {
            /* Editing existing user */
            if (_user != null)
            {
                /* If data was not changed just close fragment */
                if (!_dataWasChanged)
                {
                    CloseFragment();
                    return;
                }

                /* Getting data from fields */
                _user.FirstName = _firstName.Text;
                _user.LastName = _lastName.Text;
                _user.PatronymicName = _patronymicName.Text;
                _user.BirthDate = DateTime.ParseExact(
                    _birthDate.Text,
                    "dd.MM.yyyy",
                    System.Globalization.CultureInfo.CurrentCulture);

                /* Saving photo path */
                if (_photoWasChanged)
                {
                    if (_fromGallery)
                    {
                        _user.PhotoPath = InteractiveTimetable.Current.GetPathToImage(Activity, _currentUri);
                    }
                    else
                    {
                        _user.PhotoPath = _photo.Path;
                    }
                }

                /* Trying to save user */
                try
                {
                    InteractiveTimetable.Current.UserManager.SaveUser(_user);
                    CloseFragment();
                }
                catch (ArgumentException exception)
                {
                    /* Showing validation errors */
                    var toast = ToastHelper.GetErrorToast(Activity, exception.Message);
                    toast.SetGravity(GravityFlags.ClipVertical, 350, -300);
                    toast.Show();
                    return;
                }
            }
            /* Saving new user */
            else if(_user == null)
            {
                // TODO: Implement
            }
        }

        private void OnCancelButtonClicked(object sender, EventArgs args)
        {
            CloseFragment();
        }

        private void OnDatePickButtonClicked(object sender, EventArgs args)
        {
            var fragment = DatePickerFragment.NewInstance(
                _currentDate,
                delegate (DateTime date)
                {
                    _currentDate = date;
                    _showDateField.Text = date.ToString("dd.MM.yyyy");
                });

            fragment.Show(FragmentManager, DatePickerFragment.FragmentTag);
        }

        private void OnEditPhotoButtonClicked(object sender, EventArgs eventArgs)
        {
            if (InteractiveTimetable.Current.HasCamera)
            {
                ChoosePhotoIfHasCamera();
            }
            else
            {
                ChoosePhotoIfNoCamera();
            }
        }

        public override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            /* If user chose photo */
            if (resultCode == Result.Ok)
            {
                if (requestCode == RequestCamera)
                {
                    /* Making photo available in the gallery */
                    Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                    var contentUri = Android.Net.Uri.FromFile(_photo);
                    mediaScanIntent.SetData(contentUri);
                    Activity.SendBroadcast(mediaScanIntent);

                    _currentUri = Android.Net.Uri.FromFile(_photo);

                    /* Displaying image with resizing */
                    _bitmap = _photo.Path.LoadAndResizeBitmap(_userPhoto.Width, _userPhoto.Height);
                    if (_bitmap != null)
                    {
                        _userPhoto.SetImageBitmap(_bitmap);
                        _bitmap = null;
                    }

                    /* Setting a flag to choose method to get image path */
                    _fromGallery = false;

                    /* Dispose of the Java side bitmap. */
                    GC.Collect();

                }
                else if (requestCode == SelectFile && data != null)
                {
                    var uri = data.Data;
                    _currentUri = uri;
                    _userPhoto.SetImageURI(uri);

                    /* Setting a flag to choose method to get image path */
                    _fromGallery = true;
                }
            }

            _dataWasChanged = true;
            _photoWasChanged = true;
        }

        private void ChoosePhotoIfHasCamera()
        {
            /* Preparing dialog items */
            string[] items =
            {
                GetString(Resource.String.take_a_photo),
                GetString(Resource.String.choose_from_gallery),
                GetString(Resource.String.cancel_button)
            };

            /* Constructing dialog */
            using (var dialogBuilder = new AlertDialog.Builder(Activity))
            {
                dialogBuilder.SetTitle(GetString(Resource.String.add_photo));

                dialogBuilder.SetItems(items, (d, args) => {

                    /* Taking a photo */
                    if (args.Which == 0)
                    {
                        var intent = new Intent(MediaStore.ActionImageCapture);

                        _photo = new File(InteractiveTimetable.Current.PhotoDirectory,
                                            $"user_{Guid.NewGuid()}.jpg");

                        intent.PutExtra(
                            MediaStore.ExtraOutput,
                            Android.Net.Uri.FromFile(_photo));

                        StartActivityForResult(intent, RequestCamera);
                    }
                    /* Choosing from gallery */
                    else if (args.Which == 1)
                    {
                        ChoosePhotoIfNoCamera();
                    }
                });

                dialogBuilder.Show();
            }
        }

        private void ChoosePhotoIfNoCamera()
        {
            var intent = new Intent(
                            Intent.ActionPick,
                            MediaStore.Images.Media.ExternalContentUri);

            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);

            StartActivityForResult(
                Intent.CreateChooser(
                    intent,
                    GetString(Resource.String.choose_photo)),
                SelectFile);
        }

        private void CloseFragment()
        {
            Activity.FragmentManager.PopBackStackImmediate();
            InteractiveTimetable.Current.HideKeyboard(Activity.CurrentFocus.WindowToken);
        }

        private void OnUserDataChanged(object sender, EventArgs eventArgs)
        {
            _dataWasChanged = true;
        }
    }
}