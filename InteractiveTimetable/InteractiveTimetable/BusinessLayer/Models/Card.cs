using System.Collections.Generic;
using InteractiveTimetable.BusinessLayer.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace InteractiveTimetable.BusinessLayer.Models
{
    public class Card : BusinessEntityBase
    {
        [MaxLength(1024), NotNull]
        public string PhotoPath { get; set; }

        [NotNull]
        public bool IsDeleted { get; set; } = false;

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<ScheduleItem> ScheduleItems { get; set; } 
            = new List<ScheduleItem>();

        [ForeignKey(typeof(CardType))]
        public int CardTypeId { get; set; }

        public override bool Equals(object obj)
        {
            /* If obj is null return false */
            if (obj == null)
            {
                return false;
            }

            /* If obj can not be cast to class type return false*/
            Card card = obj as Card;
            if ((System.Object)card == null)
            {
                return false;
            }

            return PhotoPath.Equals(card.PhotoPath) &&
                   CardTypeId.Equals(card.CardTypeId);
        }
    }
}
