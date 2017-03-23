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
    public class UserDetailsEditFragment : Fragment
    {
        public static readonly string FragmentTag = "user_details_edit_fragment";

        private static readonly string UserIdKey = "current_user_id";

        public int UserId
        {
            get { return Arguments.GetInt(UserIdKey, 0); }
        }

        public static UserDetailsEditFragment NewInstance(int userId)
        {
            var userDetailsEditFragment = new UserDetailsEditFragment()
            {
                Arguments = new Bundle()
            };
            userDetailsEditFragment.Arguments.PutInt(UserIdKey, userId);

            return userDetailsEditFragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.user_details_edit, container, false);
        }
    }
}