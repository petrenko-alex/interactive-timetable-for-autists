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
    }
}
