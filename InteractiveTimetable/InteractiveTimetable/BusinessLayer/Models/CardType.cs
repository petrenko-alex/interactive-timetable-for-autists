using System.Collections.Generic;
using InteractiveTimetable.BusinessLayer.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace InteractiveTimetable.BusinessLayer.Models
{
    public class CardType : BusinessEntityBase
    {
        [MaxLength(255), NotNull]
        public string TypeName { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<Card> Cards { get; set; } 
            = new List<Card>();

        public override bool Equals(object obj)
        {
            CardType cardType = obj as CardType;
            if (cardType == null)
            {
                return false;
            }

            return TypeName.Equals(cardType.TypeName);
        }

        public bool Equals(CardType obj)
        {
            if (obj == null)
            {
                return false;
            }

            return TypeName.Equals(obj.TypeName);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                _hashCode = InitialHashValue;

                _hashCode = TypeName != null
                            ? _hashCode * HashNumber + TypeName.GetHashCode()
                            : _hashCode * HashNumber + 0;

                return _hashCode;
            }
        }
    }
}
