using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Util;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    class GridAutofitLayoutManager : GridLayoutManager
    {
        #region Internal Variables
        private static readonly int DefaultWidth = 48;
        private int _columnWidth;
        private bool _columnWidthChanged = true;
        #endregion

        #region Methods
        public GridAutofitLayoutManager(Context context, int columnWidth)
            : base(context, 1)
        {

            SetColumnWidth(CheckedColumnWidth(context, columnWidth));
        }

        public GridAutofitLayoutManager(Context context, int columnWidth, int orientation, bool reverseLayout)
            : base(context, 1, orientation, reverseLayout)
        /* Initially set spanCount to 1, it will be changed automatically later. */
        {
            SetColumnWidth(CheckedColumnWidth(context, columnWidth));
        }

        private int CheckedColumnWidth(Context context, int columnWidth)
        {
            if (columnWidth <= 0)
            {
                /* Set default columnWidth value */
                columnWidth = (int)TypedValue.ApplyDimension(
                    ComplexUnitType.Dip,
                    DefaultWidth,
                    context.Resources.DisplayMetrics
                );
            }

            return columnWidth;
        }

        public void SetColumnWidth(int newColumnWidth)
        {
            if (newColumnWidth > 0 && newColumnWidth != _columnWidth)
            {
                _columnWidth = newColumnWidth;
                _columnWidthChanged = true;
            }
        }

        public override void OnLayoutChildren(RecyclerView.Recycler recycler, RecyclerView.State state)
        {
            if (_columnWidthChanged && _columnWidth > 0)
            {
                int totalSpace;
                if (Orientation == Vertical)
                {
                    totalSpace = Width - PaddingRight - PaddingLeft;
                }
                else
                {
                    totalSpace = Height - PaddingTop - PaddingBottom;
                }
                int spanCount = Math.Max(1, totalSpace / _columnWidth);
                SpanCount = spanCount;
                _columnWidthChanged = false;
            }

            base.OnLayoutChildren(recycler, state);
        }
        #endregion
    }
}