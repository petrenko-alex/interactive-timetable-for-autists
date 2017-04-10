using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using InteractiveTimetable.Droid.ApplicationLayer;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    public class TimetableTapeFragment : Fragment
    {
        #region Constants
        public static readonly string FragmentTag = "timetable_tape_fragment";
        #endregion

        #region Widgets
        private RecyclerView _recyclerView;
        private ImageButton _editTapeButton;
        private TextView _userName;
        private ImageView _userImage;
        #endregion

        #region Internal Variables
        private RecyclerView.LayoutManager _layoutManager;
        private TimetableTapeListAdapter _tapeItemListAdapter;
        private int _userId;
        #endregion

        #region Events
        public event Action<int> EditTimetableTapeButtonClicked;
        #endregion

        #region Methods

        #region Construct Methods
        public static TimetableTapeFragment NewInstance(int userId)
        {
            var timetableTapeFragment = new TimetableTapeFragment()
            {
                _userId = userId
            };

            return timetableTapeFragment;
        }
        #endregion

        #region Event Handlers

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            /* Get views */
            _recyclerView = Activity.FindViewById<RecyclerView>(Resource.Id.tape_item_list);
            _editTapeButton = Activity.FindViewById<ImageButton>(Resource.Id.tape_edit_button);
            _userName = Activity.FindViewById<TextView>(Resource.Id.tape_user_name);
            _userImage = Activity.FindViewById<ImageView>(Resource.Id.tape_user_image);

            /* Set up layout manager */
            _layoutManager = new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false);
            _recyclerView.SetLayoutManager(_layoutManager);

            /* Set up the adapter */
            // Get cards 
            //_tapeItemListAdapter = new TimetableTapeListAdapter(Activity,);

            /* Set handlers */
            _editTapeButton.Click += OnEditTimetableTapeButtonClicked;
        }

        private void OnEditTimetableTapeButtonClicked(object sender, EventArgs e)
        {
            EditTimetableTapeButtonClicked?.Invoke(_userId);
        }

        public override View OnCreateView(
            LayoutInflater inflater,
            ViewGroup container, 
            Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.timetable_tape, container, false);
        }
        #endregion

        #endregion


    }


}