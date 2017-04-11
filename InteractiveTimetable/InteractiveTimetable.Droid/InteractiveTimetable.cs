using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views.InputMethods;
using InteractiveTimetable.BusinessLayer.Managers;
using SQLite;
using Java.IO;
using Environment = System.Environment;
using Path = System.IO.Path;
using UserManager = InteractiveTimetable.BusinessLayer.Managers.UserManager;

namespace InteractiveTimetable.Droid
{
    [Application]
    class InteractiveTimetable : Application
    {
        public static InteractiveTimetable Current { get; private set; }
        public UserManager UserManager { get; set; }
        public HospitalTripManager HospitalTripManager { get; set; }
        public ScheduleManager ScheduleManager { get; set; }

        public string AppFolder => Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public float ScreenDensity => Resources.DisplayMetrics.Density;
        public bool HasCamera { get; private set; }

        public File PhotoDirectory;
     
        private SQLiteConnection _connection;

        protected InteractiveTimetable(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            Current = this;
        }

        public override void OnCreate()
        {
            base.OnCreate();

            /* Checking camera */
            if (IsThereAnAppToTakePictures())
            {
                HasCamera = true;
                CreateDirectoryForPictures();
            }

            /* Getting a path to database file */
            var databaseFileName = "InteractiveTimetableDatabase.db3";
            var path = Path.Combine(AppFolder, databaseFileName);

            /* Creating connection and managers */
            _connection = new SQLiteConnection(path);

            UserManager = new UserManager(_connection);
            HospitalTripManager = new HospitalTripManager(_connection);
            ScheduleManager = new ScheduleManager(_connection);

            // TODO: Delete InitializeForDebugging methods
            if (UserManager.UserCount == 0)
            {
                UserManager.InitializeForDebugging(AppFolder);
            }

            if (HospitalTripManager.TripCount == 0)
            {
                HospitalTripManager.InitializeForDebugging(UserManager);
            }
        }

        private void CreateDirectoryForPictures()
        {
            PhotoDirectory 
                = new File(Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryPictures), 
                    GetString(Resource.String.app_name));

            if (!PhotoDirectory.Exists())
            {
                PhotoDirectory.Mkdirs();
            }
        }

        private bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        public void HideKeyboard(IBinder windowToken)
        {
            var inputManager = (InputMethodManager) GetSystemService(Context.InputMethodService);
            inputManager.HideSoftInputFromWindow(windowToken, 0);
        }

        public string GetPathToImage(Activity activity, Android.Net.Uri uri)
        {
            string path = null;
            string[] proj = { MediaStore.Video.Media.InterfaceConsts.Data };
            ICursor mycursor = activity.ContentResolver.Query(uri, proj, null, null, null);
            if (mycursor.MoveToFirst())
            {
                var columnIndex = mycursor.GetColumnIndexOrThrow(MediaStore.Video.Media.InterfaceConsts.Data);
                path = mycursor.GetString(columnIndex);
            }
            mycursor.Close();
            return path;
        }
    }
}