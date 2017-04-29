using System;
using Android.App;
using Android.Content;
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
        #region Constants
        public static readonly string FragmentTag = "user_details_edit_fragment";
        private static readonly string UserIdKey = "current_user_id";
        private static readonly int RequestCamera = 0;
        private static readonly int SelectFile = 1;
        private static readonly int ErrorMessageXOffset = 450;
        private static readonly int ErrorMessageYOffset = -300;
        private static readonly int UserImageSizeDp = 300;
        #endregion

        #region Widgets
        private Button _applyButton;
        private Button _cancelButton;
        private Button _editPhotoButton;
        private ImageView _userPhoto;
        private EditText _lastName;
        private EditText _firstName;
        private EditText _patronymicName;
        private EditText _birthDate;
        #endregion

        #region Events
        public event Action<int> NewUserAdded;
        public event Action<int> UserEdited;
        public event Action EditCanceled;
        #endregion

        #region Internal Variables
        private User _user;
        private DateTime _currentDate;
        private File _photo;
        private Bitmap _bitmap;
        private Android.Net.Uri _currentUri;
        #endregion

        #region Flags
        private bool _fromGallery;
        private bool _dataWasChanged;
        private bool _photoWasChanged;
        private bool _newUser;
        #endregion

        #region Properties
        public int UserId
        {
            get { return Arguments.GetInt(UserIdKey, 0); }
        }
        #endregion

        #region Methods

        #region Construct Methods
        public static UserDetailsEditFragment NewInstance(int userId)
        {
            var userDetailsEditFragment = new UserDetailsEditFragment()
            {
                Arguments = new Bundle()
            };
            userDetailsEditFragment.Arguments.PutInt(UserIdKey, userId);

            return userDetailsEditFragment;
        }
        #endregion

        #region Event Handlers
        public override View OnCreateView(
            LayoutInflater inflater,
            ViewGroup container,
            Bundle savedInstanceState)
        {
            if (container == null)
            {
                return null;
            }

            var userView = inflater.Inflate(Resource.Layout.user_details_edit, container, false);

            /* Getting widgets */
            _applyButton = userView.FindViewById<Button>(Resource.Id.apply_changes_btn);
            _cancelButton = userView.FindViewById<Button>(Resource.Id.cancel_btn);
            _editPhotoButton = userView.FindViewById<Button>(Resource.Id.edit_photo_btn);
            _userPhoto = userView.FindViewById<ImageView>(Resource.Id.user_details_photo);
            _lastName = userView.FindViewById<EditText>(Resource.Id.last_name_edit);
            _firstName = userView.FindViewById<EditText>(Resource.Id.first_name_edit);
            _patronymicName = userView.FindViewById<EditText>(Resource.Id.patronymic_name_edit);
            _birthDate = userView.FindViewById<EditText>(Resource.Id.birth_date_show);

            /* Setting button click handlers */
            _applyButton.Click += OnApplyButtonClicked;
            _cancelButton.Click += OnCancelButtonClicked;
            _editPhotoButton.Click += OnEditPhotoButtonClicked;
            _birthDate.Click += OnDatePickButtonClicked;
            _birthDate.KeyListener = null;

            /* If user is set, retrieve his data */
            if (UserId > 0)
            {
                /* Getting data */
                _user = InteractiveTimetable.Current.UserManager.GetUser(UserId);

                /* Setting last name */
                _lastName.Text = _user.LastName;

                /* Setting first name */
                _firstName.Text = _user.FirstName;

                /* Setting patronymic name */
                _patronymicName.Text = _user.PatronymicName;

                /* Setting birth date */
                _birthDate.Text = _user.BirthDate.ToString("dd.MM.yyyy");

                /* Setting photo */
                var imageSize = ImageHelper.ConvertDpToPixels(UserImageSizeDp);
                var bitmap = _user.PhotoPath.LoadAndResizeBitmap(imageSize, imageSize);
                if (bitmap != null)
                {
                    _userPhoto.SetImageBitmap(bitmap);
                }

                /* Adjust apply button */
                _applyButton.Text = GetString(Resource.String.edit_button);

                /* Adjust photo button */
                _editPhotoButton.Text = GetString(Resource.String.change_photo);

                /* Setting current date */
                _currentDate = _user.BirthDate;
            }
            else
            {
                /* Setting current date */
                _currentDate = DateTime.Today;
                _birthDate.Text = _currentDate.ToString("dd.MM.yyyy");

                /* Adjust apply button */
                _applyButton.Text = GetString(Resource.String.add_button);

                /* Adjust photo button */
                _editPhotoButton.Text = GetString(Resource.String.add_photo);
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
            }
            /* Saving new user */
            else if (_user == null)
            {
                /* Error message if trying to save empty user */
                if (!_dataWasChanged)
                {
                    var toast = ToastHelper.GetErrorToast(
                        Activity,
                        GetString(Resource.String.user_data_not_set)
                    );
                    toast.SetGravity(
                        GravityFlags.ClipVertical,
                        ErrorMessageXOffset,
                        ErrorMessageYOffset
                    );
                    toast.Show();
                    return;
                }

                /* Error message if photo is not chosen */
                if (!_photoWasChanged)
                {
                    var toast = ToastHelper.GetErrorToast(
                        Activity,
                        GetString(Resource.String.user_photo_not_set)
                    );
                    toast.SetGravity(
                        GravityFlags.ClipVertical,
                        ErrorMessageXOffset,
                        ErrorMessageYOffset
                    );
                    toast.Show();
                    return;
                }

                _newUser = true;
                _user = new User();
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
                int userId = InteractiveTimetable.Current.UserManager.SaveUser(_user);
                CloseFragment();

                /* If new user was added */
                if (_newUser)
                {
                    NewUserAdded?.Invoke(userId);
                    _newUser = false;
                    return;
                }

                /* If existing user was edited */
                UserEdited?.Invoke(userId);
            }
            catch (ArgumentException exception)
            {
                /* Showing validation errors */
                var toast = ToastHelper.GetErrorToast(Activity, exception.Message);
                toast.SetGravity(
                    GravityFlags.ClipVertical,
                    ErrorMessageXOffset,
                    ErrorMessageYOffset
                );
                toast.Show();
                return;
            }
        }

        private void OnCancelButtonClicked(object sender, EventArgs args)
        {
            EditCanceled?.Invoke();
            CloseFragment();
        }

        private void OnDatePickButtonClicked(object sender, EventArgs args)
        {
            var fragment = DatePickerFragment.NewInstance(
                _currentDate,
                delegate (DateTime date)
                {
                    _currentDate = date;
                    _birthDate.Text = date.ToString("dd.MM.yyyy");
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
                    var imageSize = ImageHelper.ConvertDpToPixels(UserImageSizeDp);
                    _bitmap = _photo.Path.LoadAndResizeBitmap(imageSize, imageSize);
                    if (_bitmap != null)
                    {
                        _userPhoto.SetImageBitmap(_bitmap);
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

                    /* Display image with resizing */
                    var imageSize = ImageHelper.ConvertDpToPixels(UserImageSizeDp);
                    var path = InteractiveTimetable.Current.GetPathToImage(Activity, uri);
                    var bitmap = path.LoadAndResizeBitmap(imageSize, imageSize);
                    if (bitmap != null)
                    {
                        _userPhoto.SetImageBitmap(bitmap);
                    }

                    /* Setting a flag to choose method to get image path */
                    _fromGallery = true;
                }

                _dataWasChanged = true;
                _photoWasChanged = true;
            }
        }

        private void OnUserDataChanged(object sender, EventArgs eventArgs)
        {
            _dataWasChanged = true;
        }
        #endregion

        #region Other Methods
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

            StartActivityForResult(
                Intent.CreateChooser(
                    intent,
                    GetString(Resource.String.choose_photo)),
                SelectFile);
        }

        private void CloseFragment()
        {
            Activity.FragmentManager.PopBackStackImmediate();
            if (Activity.CurrentFocus != null)
            {
                InteractiveTimetable.Current.HideKeyboard(Activity.CurrentFocus.WindowToken);
            }
        }
        #endregion

        #endregion
    }
}