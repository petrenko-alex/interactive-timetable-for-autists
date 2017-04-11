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
using InteractiveTimetable.BusinessLayer.Models;
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
        private IList<Card> _tapeCards;
        private int _userId;
        #endregion

        #region Events
        public event Action<int> EditTimetableTapeButtonClicked;
        #endregion

        #region Methods

        #region Construct Methods
        public static TimetableTapeFragment NewInstance(int userId, IList<Card> tapeCards)
        {
            var timetableTapeFragment = new TimetableTapeFragment()
            {
                _userId = userId,
                _tapeCards = tapeCards,
            };

            return timetableTapeFragment;
        }
        #endregion

        #region Event Handlers
        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            /* Get data */
            var user = InteractiveTimetable.Current.UserManager.GetUser(_userId);

            /* Get views */
            _recyclerView = Activity.FindViewById<RecyclerView>(Resource.Id.tape_item_list);
            _editTapeButton = Activity.FindViewById<ImageButton>(Resource.Id.tape_edit_button);
            _userName = Activity.FindViewById<TextView>(Resource.Id.tape_user_name);
            _userImage = Activity.FindViewById<ImageView>(Resource.Id.tape_user_image);

            /* Set up layout manager */
            _layoutManager = new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false);
            _recyclerView.SetLayoutManager(_layoutManager);

            /* Set widgets data */
            _userName.Text = user.FirstName;
            _userImage.SetImageURI(Android.Net.Uri.Parse(user.PhotoPath));

            /* Set up the adapter */
            _tapeItemListAdapter = new TimetableTapeListAdapter(Activity, _tapeCards);
            _tapeItemListAdapter.ItemClick += OnItemClick;
            _tapeItemListAdapter.ItemLongClick += OnItemLongClick;
            _recyclerView.SetAdapter(_tapeItemListAdapter);

            /* Set handlers */
            _editTapeButton.Click += OnEditTimetableTapeButtonClicked;

            // TODO: Show message if no cards yet
        }

        private void OnItemLongClick(int tapeCardId, int positionInList)
        {
            throw new NotImplementedException();
        }

        private void OnItemClick(int tapeCardId, int positionInList)
        {
            throw new NotImplementedException();
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

        public override void OnDestroy()
        {
            _tapeItemListAdapter.ItemClick -= OnItemClick;
            _tapeItemListAdapter.ItemLongClick -= OnItemLongClick;
            _editTapeButton.Click -= OnEditTimetableTapeButtonClicked;
            GC.Collect();

            base.OnDestroy();
        }

        #endregion

        #region Other Methods
        public void DataSetChanged()
        {
            // TODO: Implement when need to change schedule after timetable created or changed
        }
        #endregion

        #endregion
    }
}