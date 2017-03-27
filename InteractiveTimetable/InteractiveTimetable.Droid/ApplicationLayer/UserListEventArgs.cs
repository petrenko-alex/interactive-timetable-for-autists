using System;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    public class UserListEventArgs : EventArgs
    {
        public int UserId { get; set; }
        public int PositionInList { get; set; }
    }
}