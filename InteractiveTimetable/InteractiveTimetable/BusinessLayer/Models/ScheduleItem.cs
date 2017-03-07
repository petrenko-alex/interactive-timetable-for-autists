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
            /* If obj is null return false */
            if (obj == null)
            {
                return false;
            }

            /* If obj can not be cast to class type return false*/
            ScheduleItem item = obj as ScheduleItem;
            if ((System.Object)item == null)
            {
                return false;
            }

            return OrderNumber.Equals(item.OrderNumber) &&
                   ScheduleId.Equals(item.ScheduleId) &&
                   CardId.Equals(item.CardId);
        }
    }
}
