using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Android.App;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AndroidViewAnimations;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.Droid.ApplicationLayer;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    public class TimetableTapeFragment : Fragment
    {
        #region Constants
        public static readonly string FragmentTag = "timetable_tape_fragment";
        private static readonly int AnimationDuration = 700;
        private static readonly int ScrollTimer = 1000;
        #endregion

        #region Widgets
        private RecyclerView _recyclerView;
        private ImageButton _editTapeButton;
        private TextView _userName;
        private ImageView _userImage;
        private FrameLayout _staticGoalCardFrame;
        private ImageView _staticGoalCard;
        #endregion

        #region Internal Variables
        private XamarinRecyclerViewOnScrollListener _scrollListener;
        private LockableLinearLayoutManager _layoutManager;
        private TimetableTapeListAdapter _tapeItemListAdapter;
        private IList<ScheduleItem> _tapeItems;
        private int _userId;
        private bool _isLocked;
        private Timer _timer;
        private int _itemWidth;
        #endregion

        #region Events
        public event Action<int> EditTimetableTapeButtonClicked;
        public event Action<object, EventArgs> ClickedWhenLocked;
        #endregion

        #region Methods

        #region Construct Methods
        public static TimetableTapeFragment NewInstance(int userId, IList<ScheduleItem> tapeItems)
        {
            var timetableTapeFragment = new TimetableTapeFragment()
            {
                _userId = userId,
                _tapeItems = tapeItems,
                _isLocked =  false
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
            _staticGoalCard = Activity.FindViewById<ImageView>(Resource.Id.static_goal_card);
            _staticGoalCardFrame =
                    Activity.FindViewById<FrameLayout>(Resource.Id.static_goal_card_frame);
            GenerateNewIdsForViews();

            /* Set up layout manager */
            _layoutManager = new LockableLinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false);
            _recyclerView.SetLayoutManager(_layoutManager);

            /* Set up scroll listener for recycler view */
            _scrollListener = new XamarinRecyclerViewOnScrollListener(
                _layoutManager,
                _tapeItems.Count - 1
            );
            _scrollListener.LastItemIsVisible += OnLastItemIsVisible;
            _scrollListener.LastItemIsHidden += OnLastItemIsHidden;
            _recyclerView.AddOnScrollListener(_scrollListener);

            /* Set widgets data */
            _userName.Text = user.FirstName;
            _userImage.SetImageURI(Android.Net.Uri.Parse(user.PhotoPath));
            // TODO: Change to normal load - _staticGoalCard.SetImageURI(Android.Net.Uri.Parse(_tapeItems.Last().PhotoPath));
            var imageSize = ImageHelper.ConvertDpToPixels(
                140,
                InteractiveTimetable.Current.ScreenDensity
            );
            var card = InteractiveTimetable.Current.ScheduleManager.Cards.
                                            GetCard(_tapeItems.Last().CardId);
            var bitmap = card.PhotoPath.LoadAndResizeBitmap(imageSize, imageSize);
            _staticGoalCard.SetImageBitmap(bitmap);

            /* Set up the adapter */
            _tapeItemListAdapter = new TimetableTapeListAdapter(Activity, _tapeItems);
            _tapeItemListAdapter.ItemClick += OnItemClick;
            _tapeItemListAdapter.ItemLongClick += OnItemLongClick;
            _recyclerView.SetAdapter(_tapeItemListAdapter);

            /* Set handlers */
            _editTapeButton.Click += OnEditTimetableTapeButtonClicked;

            /* Set view settings */
            _recyclerView.SetClipToPadding(false);

            // TODO: Show message if no cards yet
        }

        private void OnLastItemIsHidden()
        {
            _staticGoalCardFrame.Visibility = ViewStates.Visible;
        }

        private void OnLastItemIsVisible()
        {
            _staticGoalCardFrame.Visibility = ViewStates.Gone;
        }

        private void OnItemLongClick(
            TimetableTapeItemViewHolder viewHolder,
            int tapeItemId,
            int positionInList)
        {
            /* If tape is locked send signal */
            if (_isLocked)
            {
                OnLockedClicked();
                return;
            }

            var scheduleItem = InteractiveTimetable.Current.ScheduleManager.
                                                    GetScheduleItem(tapeItemId);

            if (scheduleItem.IsCompleted)
            {
                /* Check if schedule completed */
                bool isScheduleCompleted = InteractiveTimetable.Current.ScheduleManager.
                                IsScheduleCompleted(scheduleItem.ScheduleId);

                if (isScheduleCompleted)
                {
                    /* Mark as uncompleted in database */
                    InteractiveTimetable.Current.ScheduleManager.
                                         UncompleteSchedule(scheduleItem.ScheduleId);
                }

                /* Mark as uncomplete in database and data set */
                InteractiveTimetable.Current.ScheduleManager.UncompleteScheduleItem(tapeItemId);
                _tapeItems[positionInList].IsCompleted = false;

                /* Put off a green tick with animation */
                PutOffGreenTick(viewHolder.ItemImage);
                YoYo.With(Techniques.Landing).Duration(AnimationDuration).PlayOn(viewHolder.ItemImage);
            }
        }

        private void OnItemClick(
            TimetableTapeItemViewHolder viewHolder, 
            int tapeItemId, 
            int positionInList)
        {
            /* If tape is locked send signal */
            if (_isLocked)
            {
                OnLockedClicked();
                return;
            }

            /* Get data */
            var scheduleItem = InteractiveTimetable.Current.ScheduleManager.
                                                    GetScheduleItem(tapeItemId);

            if (!scheduleItem.IsCompleted)
            {
                /* Mark as complete in database and data set */
                InteractiveTimetable.Current.ScheduleManager.CompleteScheduleItem(tapeItemId);
                _tapeItems[positionInList].IsCompleted = true;

                /* Put on a green tick with animation */
                PutOnGreenTick(viewHolder.ItemImage);
                YoYo.With(Techniques.Landing).Duration(AnimationDuration).PlayOn(viewHolder.ItemImage);

                /* Timer to scroll */
                _itemWidth = viewHolder.ItemImage.Width;
                _timer = new Timer(ScrollTimer);
                _timer.Elapsed += (sender, e) => NeedToScroll(sender, e, positionInList);
                _timer.Start();

                /* Check if schedule is completed */
                bool isScheduleCompleted = InteractiveTimetable.Current.ScheduleManager.
                                IsScheduleCompleted(scheduleItem.ScheduleId);

                if (isScheduleCompleted)
                {
                    /* Mark as completed in database */
                    InteractiveTimetable.Current.ScheduleManager.
                                         CompleteSchedule(scheduleItem.ScheduleId);

                    /* Show animation */
                    var goalImage = _tapeItemListAdapter.GoalViewHolder.ItemImage;
                    YoYo.With(Techniques.RubberBand).Duration(1000).PlayOn(goalImage);
                    YoYo.With(Techniques.Shake).Duration(1000).PlayOn(goalImage);
                    YoYo.With(Techniques.Wobble).Duration(1000).PlayOn(goalImage);

                    /* Timer to hide tape and show info message */
                }
            }
        }
    
        private void NeedToScroll(object sender, ElapsedEventArgs args, int positionInList)
        {
            _timer.Stop();

            Activity.RunOnUiThread(() =>
            {
                /* Set right padding */
                //int rightPadding = _tapeItems.Count * _itemWidth;
                int rightPadding = _recyclerView.Width;
                _recyclerView.SetPadding(0, 0, rightPadding, 0);
                Console.WriteLine($"RecyclerView Width {_recyclerView.Width}");

                ScrollToHideCompletedActivity();
                //SmoothScrollToHideCompletedActivities();
            });
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
            _recyclerView.ClearOnScrollListeners();
            GC.Collect();

            base.OnDestroy();
        }

        private void OnLockedClicked()
        {
            ClickedWhenLocked?.Invoke(this, null);
        }
        #endregion

        #region Other Methods
        public void DataSetChanged()
        {
            // TODO: Implement when need to change schedule after timetable created or changed
        }

        public void GenerateNewIdsForViews()
        {
            int newId = View.GenerateViewId();

            /* New id for _recyclerView */
            _recyclerView.Id = newId;
            _recyclerView = Activity.FindViewById<RecyclerView>(newId);

            /* New id for _editTapeButton */
            newId = View.GenerateViewId();
            _editTapeButton.Id = newId;
            _editTapeButton = Activity.FindViewById<ImageButton>(newId);

            /* New id for _userName */
            newId = View.GenerateViewId();
            _userName.Id = newId;
            _userName = Activity.FindViewById<TextView>(newId);

            /* New id for _userImage */
            newId = View.GenerateViewId();
            _userImage.Id = newId;
            _userImage = Activity.FindViewById<ImageView>(newId);

            /* New id for _staticGoalCard */
            newId = View.GenerateViewId();
            _staticGoalCard.Id = newId;
            _staticGoalCard = Activity.FindViewById<ImageView>(newId);

            /* New id for _staticGoalCardFrame */
            newId = View.GenerateViewId();
            _staticGoalCardFrame.Id = newId;
            _staticGoalCardFrame = Activity.FindViewById<FrameLayout>(newId);
        }

        public void LockFragment()
        {
            _isLocked = true;
            _layoutManager.IsScrollEnabled = false;
        }

        public void UnlockFragment()
        {
            _isLocked = false;
            _layoutManager.IsScrollEnabled = true;
        }

        private void PutOnGreenTick(ImageView imageView)
        {
            var layers = new Drawable[2];
            layers[0] = imageView.Drawable;
            layers[1] = Resources.GetDrawable(Resource.Drawable.green_tick);
            var layerDrawable = new LayerDrawable(layers);
            imageView.SetImageDrawable(layerDrawable);
        }

        private void PutOffGreenTick(ImageView imageView)
        {
            Drawable cardImage = null;
            var layer = imageView.Drawable as LayerDrawable;
            if (layer != null)
            {
                cardImage = layer.GetDrawable(0);
            }
            imageView.SetImageDrawable(cardImage);
        }

        private void SmoothScrollToHideCompletedActivities()
        {
            // TODO: Not stable when amount of items in tape is 20 and higher
            var lastCompletedActivityNumber =  GetLastCompletedActivityNumber();

            /* If have completed activities need to scroll */
            if (lastCompletedActivityNumber > 0)
            {
                /* Calculate scrollX */
                int scrollX = (_itemWidth + 7) * lastCompletedActivityNumber + 1;
                _recyclerView.SmoothScrollBy(scrollX - _scrollListener.ScrollX1, 0);
            }
        }

        private void ScrollToHideCompletedActivity()
        {
            var lastCompletedActivityNumber = GetLastCompletedActivityNumber();

            Console.WriteLine( $"Last visible item {_layoutManager.FindLastVisibleItemPosition()}");
            Console.WriteLine($"Last completely visible item {_layoutManager.FindLastCompletelyVisibleItemPosition()}");

            /* If have completed activities need to scroll */
            if (lastCompletedActivityNumber > 0)
            {
                _layoutManager.ScrollToPositionWithOffset(lastCompletedActivityNumber, 0);
            }
        }

        private int GetLastCompletedActivityNumber()
        {
            /* Find last completed activity */
            var lastCompletedActivityNumber = 0;
            int activityCount = _tapeItems.Count;
            for (int i = 0; i < activityCount; ++i)
            {
                if (!_tapeItems[lastCompletedActivityNumber].IsCompleted)
                {
                    break;
                }
                else
                {
                    lastCompletedActivityNumber = i + 1;
                }
            }

            return lastCompletedActivityNumber;
        }
        #endregion

        #endregion
    }
}