using System;
using Android.Support.V7.Widget;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    internal class XamarinRecyclerViewOnScrollListener : RecyclerView.OnScrollListener
    {
        #region Events
        public event Action LastItemIsVisible;
        public event Action LastItemIsHidden;
        #endregion

        #region Internal Variables
        private readonly LinearLayoutManager _layoutManager;
        private readonly int _lastItemPosition;
        #endregion

        #region Properties
        public int ScrollX1 { get; set; }
        #endregion

        public XamarinRecyclerViewOnScrollListener(
            LinearLayoutManager layoutManager, 
            int lastItemPosition)
        {
            _layoutManager = layoutManager;
            _lastItemPosition = lastItemPosition;
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            base.OnScrolled(recyclerView, dx, dy);

            if (ScrollX1 + dx < 0)
            {
                ScrollX1 = 0;
            }
            else
            {
                ScrollX1 = ScrollX1 + dx;
            }
            
            /* Check whether the last item is visible */
            var lastVisibleItemPosition = _layoutManager.FindLastVisibleItemPosition();
            if (lastVisibleItemPosition == _lastItemPosition)
            {
                LastItemIsVisible?.Invoke();
            }
            else if (lastVisibleItemPosition < _lastItemPosition)
            {
                LastItemIsHidden?.Invoke();
            }
        }
    }
}