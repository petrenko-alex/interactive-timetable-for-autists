using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace InteractiveTimetable.Droid.UserInterfaceLayer
{
    public class InfoFragment : Fragment
    {
        #region Constants
        public static readonly string FragmentTag = "info_fragment";
        #endregion

        #region Widgets
        private TextView _infoTextView;
        #endregion

        #region Internal Variables
        private string _infoMessage;
        #endregion

        #region Methods

        #region Construct Methods
        public static InfoFragment NewInstance(string message)
        {
            return new InfoFragment()
            {
                _infoMessage = message
            };
        }
        #endregion

        #region Event Handlers
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                return null;
            }

            /* Preparing layout */
            View infoView = inflater.Inflate(Resource.Layout.info, container, false);
            _infoTextView = infoView.FindViewById<TextView>(Resource.Id.info_message);
            _infoTextView.Text = _infoMessage;

            return infoView;
        }
        #endregion

        #endregion

    }
}