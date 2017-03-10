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

        public override bool Equals(object obj)
        {
            ScheduleItem item = obj as ScheduleItem;
            if (item == null)
            {
                return false;
            }

            return OrderNumber.Equals(item.OrderNumber) &&
                   ScheduleId.Equals(item.ScheduleId) &&
                   CardId.Equals(item.CardId);
        }

        public bool Equals(ScheduleItem obj)
        {
            if (obj == null)
            {
                return false;
            }

            return OrderNumber.Equals(obj.OrderNumber) &&
                   ScheduleId.Equals(obj.ScheduleId) &&
                   CardId.Equals(obj.CardId);
        }
    }
}
