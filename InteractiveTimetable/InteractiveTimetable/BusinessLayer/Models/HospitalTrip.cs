using System;
using System.Collections.Generic;
using InteractiveTimetable.BusinessLayer.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace InteractiveTimetable.BusinessLayer.Models
{
    public class HospitalTrip : BusinessEntityBase
    {
        [NotNull]
        public int Number { get; set; }

        [NotNull]
        public DateTime StartDate { get; set; }

        [NotNull]
        public DateTime FinishDate { get; set; }

        [NotNull]
        public bool IsDeleted { get; set; } = false;

        [ForeignKey(typeof(User))]
        public int UserId { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<Diagnostic> Diagnostics { get; set; }
    }
}
