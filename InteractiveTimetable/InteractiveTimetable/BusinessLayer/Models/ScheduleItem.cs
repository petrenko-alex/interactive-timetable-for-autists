using InteractiveTimetable.BusinessLayer.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace InteractiveTimetable.BusinessLayer.Models
{
    public class ScheduleItem : BusinessEntityBase
    {
        [NotNull]
        public int OrderNumber { get; set; }

        [ForeignKey(typeof(Schedule))]
        public int ScheduleId { get; set; }

        [ForeignKey(typeof(Card))]
        public int CardId { get; set; }
    }
}
