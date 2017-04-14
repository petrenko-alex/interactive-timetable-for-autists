using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    internal class LockableLinearLayoutManager : LinearLayoutManager
    {
        #region Events
        public event Action TriedToScrollWhenLocked;
        #endregion

        #region Properties
        public bool IsScrollEnabled { get; set; } = true;
        #endregion

        #region Methods

        #region Construct Methods
        public LockableLinearLayoutManager(IntPtr javaReference, JniHandleOwnership transfer) 
            : base(javaReference, transfer) { }
        public LockableLinearLayoutManager(Context context) 
            : base(context) { }
        public LockableLinearLayoutManager(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) 
            : base(context, attrs, defStyleAttr, defStyleRes) { }
        public LockableLinearLayoutManager(Context context, int orientation, bool reverseLayout) 
            : base(context, orientation, reverseLayout) { }
        #endregion

        #region Other Methods
        public override bool CanScrollHorizontally()
        {
            if (!IsScrollEnabled)
            {
                TriedToScrollWhenLocked?.Invoke();
            }

            return IsScrollEnabled && base.CanScrollHorizontally();
        }

        public override bool CanScrollVertically()
        {
            if (!IsScrollEnabled)
            {
                TriedToScrollWhenLocked?.Invoke();
            }

            return IsScrollEnabled && base.CanScrollVertically();
        }
        #endregion

        #endregion
    }
}