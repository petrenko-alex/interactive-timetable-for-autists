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
    }
}
