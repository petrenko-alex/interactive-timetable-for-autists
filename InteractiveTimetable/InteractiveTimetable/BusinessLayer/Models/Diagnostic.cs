using System;
using System.Collections.Generic;
using InteractiveTimetable.BusinessLayer.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace InteractiveTimetable.BusinessLayer.Models
{
    public class Diagnostic : BusinessEntityBase
    {
        [NotNull]
        public DateTime Date { get; set; }

        [NotNull]
        public bool IsDeleted { get; set; } = false;

        [ForeignKey(typeof(HospitalTrip))]
        public int HospitalTripId { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<CriterionGrade> CriterionGrades { get; set; }
    }
}
