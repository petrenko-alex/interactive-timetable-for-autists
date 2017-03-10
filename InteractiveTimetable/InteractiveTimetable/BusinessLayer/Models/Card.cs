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
            Card card = obj as Card;
            if (card == null)
            {
                return false;
            }

            return PhotoPath.Equals(card.PhotoPath) &&
                   CardTypeId.Equals(card.CardTypeId);
        }

        public bool Equals(Card obj)
        {
            if (obj == null)
            {
                return false;
            }

            return PhotoPath.Equals(obj.PhotoPath) &&
                   CardTypeId.Equals(obj.CardTypeId);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                _hashCode = InitialHashValue;

                _hashCode = PhotoPath != null
                            ? _hashCode * HashNumber + PhotoPath.GetHashCode()
                            : _hashCode * HashNumber + 0;

                _hashCode = _hashCode * HashNumber + CardTypeId.GetHashCode();

                return _hashCode;
            }
        }
    }
}
