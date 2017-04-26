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
        private LinearLayout _infoLayout;
        private Button _returnScheduleButton;
        private TextView _infoText;
        #endregion

        #region Internal Variables
        private XamarinRecyclerViewOnScrollListener _scrollListener;
        private LockableLinearLayoutManager _layoutManager;
        private TimetableTapeListAdapter _tapeItemListAdapter;
        private IList<ScheduleItem> _tapeItems;
        private bool _isLocked;
        private int _itemWidth;
        private Timer _scrollTimer;
        #endregion

        #region Events
        public event Action<int, int, IList<Card>> EditTimetableTapeButtonClicked;
        public event Action<object, EventArgs> ClickedWhenLocked;
        #endregion

        #region Properties
        public int TapeNumber { get; set; }
        public Schedule CurrentSchedule { get; private set; }
        public int UserId { get; private set; }
        #endregion

        #region Methods

        #region Construct Methods
        public static TimetableTapeFragment NewInstance(
            int userId, 
            IList<ScheduleItem> tapeItems, 
            int fragmentNumber)
        {
            var timetableTapeFragment = new TimetableTapeFragment()
            {
                UserId = userId,
                _tapeItems = tapeItems,
                TapeNumber =  fragmentNumber,
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
            var user = InteractiveTimetable.Current.UserManager.GetUser(UserId);

            /* Get views */
            _recyclerView = Activity.FindViewById<RecyclerView>(Resource.Id.tape_item_list);
            _editTapeButton = Activity.FindViewById<ImageButton>(Resource.Id.tape_edit_button);
            _userName = Activity.FindViewById<TextView>(Resource.Id.tape_user_name);
            _userImage = Activity.FindViewById<ImageView>(Resource.Id.tape_user_image);
            _staticGoalCard = Activity.FindViewById<ImageView>(Resource.Id.static_goal_card);
            _staticGoalCardFrame =
                    Activity.FindViewById<FrameLayout>(Resource.Id.static_goal_card_frame);
            _infoLayout = Activity.FindViewById<LinearLayout>(Resource.Id.tape_info_layout);
            _returnScheduleButton = Activity.FindViewById<Button>(Resource.Id.return_schedule_button);
            _infoText = Activity.FindViewById<TextView>(Resource.Id.tape_info);
            GenerateNewIdsForViews();

            /* Set up layout manager */
            _layoutManager = new LockableLinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false);
            _recyclerView.SetLayoutManager(_layoutManager);

            /* Set up the adapter */
            _tapeItemListAdapter = new TimetableTapeListAdapter(Activity, _tapeItems);
            _tapeItemListAdapter.ItemClick += OnItemClick;
            _tapeItemListAdapter.ItemLongClick += OnItemLongClick;
            _recyclerView.SetAdapter(_tapeItemListAdapter);

            /* Set widgets data */
            _userName.Text = user.FirstName;
            _userImage.SetImageURI(Android.Net.Uri.Parse(user.PhotoPath));

            /* If has schedule for today */
            if (_tapeItems.Any())
            {
                CurrentSchedule = InteractiveTimetable.Current.ScheduleManager.
                                                    GetSchedule(_tapeItems[0].ScheduleId);

                /* Set up scroll listener for recycler view */
                _scrollListener = new XamarinRecyclerViewOnScrollListener(
                    _layoutManager,
                    _tapeItems.Count - 1
                );
                _scrollListener.LastItemIsVisible += OnLastItemIsVisible;
                _scrollListener.LastItemIsHidden += OnLastItemIsHidden;
                _recyclerView.AddOnScrollListener(_scrollListener);

                /* Set static goal card */
                // TODO: Change to normal load - _staticGoalCard.SetImageURI(Android.Net.Uri.Parse(_tapeItems.Last().PhotoPath));
                var imageSize = ImageHelper.ConvertDpToPixels(
                    140,
                    InteractiveTimetable.Current.ScreenDensity
                );
                var card = InteractiveTimetable.Current.ScheduleManager.Cards.
                                                GetCard(_tapeItems.Last().CardId);
                var bitmap = card.PhotoPath.LoadAndResizeBitmap(imageSize, imageSize);
                _staticGoalCard.SetImageBitmap(bitmap);

                /* Set up timers */
                _scrollTimer = new Timer(ScrollTimer);

                /* If timetable is completed */
                if (CurrentSchedule.IsCompleted)
                {
                    NeedToHideTimetableTape(this, null);
                }
            }
            /* If no schedule for today */
            else
            {
                NeedToHideTimetableTape(this, null);

                _infoText.Text = GetString(Resource.String.no_timetable_for_today);
                _returnScheduleButton.Visibility = ViewStates.Gone;
            }

            /* Set handlers */
            _editTapeButton.Click += OnEditTimetableTapeButtonClicked;
            _returnScheduleButton.Click += OnReturnScheduleButtonClicked;

            /* Set view settings */
            _recyclerView.SetClipToPadding(false);
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
                _scrollTimer.Stop();
                _scrollTimer.Elapsed += (sender, e) => NeedToScroll(sender, e, positionInList);
                _scrollTimer.Start();
            }
        }

        private void CheckIfScheduleIsCompleted()
        {
            /* Check if schedule is completed */
            bool isScheduleCompleted = InteractiveTimetable.Current.ScheduleManager.
                            IsScheduleCompleted(CurrentSchedule.Id);

            if (isScheduleCompleted)
            {
                /* Mark as completed in database */
                InteractiveTimetable.Current.ScheduleManager.
                                     CompleteSchedule(CurrentSchedule.Id);

                /* Show animation */
                var goalImage = _tapeItemListAdapter.GoalViewHolder.ItemImage;
                YoYo.With(Techniques.RubberBand).Duration(1000).PlayOn(goalImage);
                YoYo.With(Techniques.Shake).Duration(1000).PlayOn(goalImage);
                YoYo.With(Techniques.Wobble).Duration(1000).PlayOn(goalImage);

                /* Timer to hide tape and show info message */
                var hideTimer = new Timer(ScrollTimer * 2);
                hideTimer.Elapsed += NeedToHideTimetableTape;
                hideTimer.Start();

                /* Set text for info message */
                _infoText.Text = GetString(Resource.String.schedule_is_completed);
            }
        }

        private void NeedToHideTimetableTape(object sender, ElapsedEventArgs e)
        {
            var timer = sender as Timer;
            timer?.Stop();

            Activity.RunOnUiThread(() =>
            {
                _staticGoalCardFrame.Visibility = ViewStates.Gone;
                _recyclerView.Visibility = ViewStates.Gone;
                YoYo.With(Techniques.FadeIn).Duration(AnimationDuration).PlayOn(_infoLayout);
                
                _infoLayout.Visibility = ViewStates.Visible;
                _returnScheduleButton.Visibility = ViewStates.Visible;
            });
        }

        private void OnReturnScheduleButtonClicked(object sender, EventArgs e)
        {
            /* If tape is locked send signal */
            if (_isLocked)
            {
                OnLockedClicked();
                return;
            }

            _infoLayout.Visibility = ViewStates.Gone;
            _recyclerView.Visibility = ViewStates.Visible;
            _recyclerView.SmoothScrollToPosition(0);
            _staticGoalCardFrame.Visibility = ViewStates.Visible;
        }

        private void NeedToScroll(object sender, ElapsedEventArgs args, int positionInList)
        {
            var timer = sender as Timer;
            timer?.Stop();

            Activity.RunOnUiThread(() =>
            {
                /* Set right padding */
                int rightPadding = _recyclerView.Width;
                _recyclerView.SetPadding(0, 0, rightPadding, 0);

                //ScrollToHideCompletedActivity();
                SmoothScrollToHideCompletedActivities();

                CheckIfScheduleIsCompleted();
            });
        }

        private void OnEditTimetableTapeButtonClicked(object sender, EventArgs e)
        {
            /* Get card ids */
            var cards = _tapeItems.
                    Select(x => InteractiveTimetable.Current.ScheduleManager.Cards.
                                                     GetCard(x.CardId)).
                    ToList();

            EditTimetableTapeButtonClicked?.Invoke(UserId, TapeNumber, cards);
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

        public void SetSchedule(IList<ScheduleItem> tapeItems)
        {
            /* Show timetable tape if hidden */
            if (_infoLayout.Visibility == ViewStates.Visible)
            {
                _infoLayout.Visibility = ViewStates.Gone;
                _returnScheduleButton.Visibility = ViewStates.Gone;

                _staticGoalCardFrame.Visibility = ViewStates.Visible;
                _recyclerView.Visibility = ViewStates.Visible;
            }

            /* Set data */
            _tapeItems = tapeItems;

            CurrentSchedule = InteractiveTimetable.Current.ScheduleManager.
                                                    GetSchedule(_tapeItems[0].ScheduleId);

            /* Reset scroll listener */
            if (_scrollListener != null)
            {
                _scrollListener.LastItemIsVisible -= OnLastItemIsVisible;
                _scrollListener.LastItemIsHidden -= OnLastItemIsHidden;
                _recyclerView.RemoveOnScrollListener(_scrollListener);
            }
            _scrollListener = new XamarinRecyclerViewOnScrollListener(
                _layoutManager,
                _tapeItems.Count - 1
            );
            _scrollListener.LastItemIsVisible += OnLastItemIsVisible;
            _scrollListener.LastItemIsHidden += OnLastItemIsHidden;            
            _recyclerView.AddOnScrollListener(_scrollListener);

            /* Set static goal card */
            // TODO: Change to normal load - _staticGoalCard.SetImageURI(Android.Net.Uri.Parse(_tapeItems.Last().PhotoPath));
            var imageSize = ImageHelper.ConvertDpToPixels(
                140,
                InteractiveTimetable.Current.ScreenDensity
            );
            var card = InteractiveTimetable.Current.ScheduleManager.Cards.
                                            GetCard(_tapeItems.Last().CardId);
            var bitmap = card.PhotoPath.LoadAndResizeBitmap(imageSize, imageSize);
            _staticGoalCard.SetImageBitmap(bitmap);

            /* Set up timers */
            _scrollTimer = new Timer(ScrollTimer);

            /* Set adapter */
            _tapeItemListAdapter.TapeItems = _tapeItems;
            _tapeItemListAdapter.NotifyDataSetChanged();
            _recyclerView.SmoothScrollToPosition(0);
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

            /* New id for _infoLayout */
            newId = View.GenerateViewId();
            _infoLayout.Id = newId;
            _infoLayout = Activity.FindViewById<LinearLayout>(newId);

            /* New id for _returnScheduleButton */
            newId = View.GenerateViewId();
            _returnScheduleButton.Id = newId;
            _returnScheduleButton = Activity.FindViewById<Button>(newId);

            /* New id for _infoText */
            newId = View.GenerateViewId();
            _infoText.Id = newId;
            _infoText = Activity.FindViewById<TextView>(newId);
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

        public void RefreshUserInfo()
        {
            /* Get latest data */
            var user = InteractiveTimetable.Current.UserManager.GetUser(UserId);

            /* Set name */
            _userName.Text = user.FirstName;

            /* Set photo */
            var bitmap = user.PhotoPath.LoadAndResizeBitmap(_userImage.Width, _userImage.Height);
            if (bitmap != null)
            {
                _userImage.SetImageBitmap(bitmap);
            }
        }
        #endregion

        #endregion
    }
}