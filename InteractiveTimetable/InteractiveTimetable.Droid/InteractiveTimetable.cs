using System;
using System.IO;
using Android.App;
using Android.Runtime;
using SQLite;
using InteractiveTimetable.BusinessLayer.Managers;
using Environment = System.Environment;


namespace InteractiveTimetable.Droid
{
    [Application]
    class InteractiveTimetable : Application
    {
        public static InteractiveTimetable Current { get; private set; }
        public UserManager UserManager { get; set; }

        private SQLiteConnection _connection;

        protected InteractiveTimetable(IntPtr javaReference,
                                       JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            Current = this;
        }

        public override void OnCreate()
        {
            base.OnCreate();

            /* Getting a path to database file */
            var databaseFileName = "InteractiveTimetableDatabase.db3";
            var libraryPath =
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            var path = Path.Combine(libraryPath, databaseFileName);

            /* Creating connection and managers */
            _connection = new SQLiteConnection(path);

            UserManager = new UserManager(_connection);
        }
    }
}