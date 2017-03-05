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
            /* If obj is null return false */
            if (obj == null)
            {
                return false;
            }

            /* If obj can not be cast to class type return false*/
            CardType cardType = obj as CardType;
            if ((System.Object)cardType == null)
            {
                return false;
            }

            return TypeName.Equals(cardType.TypeName);
        }
    }
}
