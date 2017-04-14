using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    [Register("petrenko.alexander.InteractiveTimetable.Droid.ApplicationLayer.LockableScrollView")]
    internal class LockableScrollView : ScrollView
    {
        #region Properties
        public bool IsScrollEnabled { get; set; } = true;
        #endregion

        #region Methods

        #region Construct Methods
        public LockableScrollView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) {}

        public LockableScrollView(Context context)
            : base(context) {}

        public LockableScrollView(Context context, IAttributeSet attrs)
            : base(context, attrs) {}

        public LockableScrollView(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr) {}

        public LockableScrollView(Context context, IAttributeSet attrs, int defStyleAttr,
                                  int defStyleRes)
            : base(context, attrs, defStyleAttr, defStyleRes) {}
        #endregion

        #region Event Handlers
        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            if (!IsScrollEnabled)
            {
                return false;
            }

            return base.OnInterceptTouchEvent(ev);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    {
                        /* If scrolling is enabled pass the event to the superclass */
                        if (IsScrollEnabled)
                        {
                            return base.OnTouchEvent(e);
                        }

                        /* If not return false */
                        return IsScrollEnabled;
                    }
                default:
                    {
                        return base.OnTouchEvent(e);
                    }
            }
        }
        #endregion

        #region Other Methods
        public override bool CanScrollHorizontally(int direction)
        {
            return IsScrollEnabled && base.CanScrollHorizontally(direction);
        }

        public override bool CanScrollVertically(int direction)
        {
            return IsScrollEnabled && base.CanScrollVertically(direction);
        }
        #endregion

        #endregion
    }
}