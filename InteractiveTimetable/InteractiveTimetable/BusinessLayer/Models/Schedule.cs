using System;
using System.Collections.Generic;
using InteractiveTimetable.BusinessLayer.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace InteractiveTimetable.BusinessLayer.Models
{
    public class Schedule : BusinessEntityBase
    {
        [NotNull]
        public DateTime CreateTime { get; set; } = DateTime.Now.Date;

        public DateTime FinishTime { get; set; }

        [NotNull]
        public bool IsCompleted { get; set; } = false;

        [ForeignKey(typeof(User))]
        public int UserId { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<ScheduleItem> ScheduleItems { get; set; } 
            = new List<ScheduleItem>();
    }
}
