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
            = new List<Diagnostic>();

        public override bool Equals(object obj)
        {
            HospitalTrip hospitalTrip = obj as HospitalTrip;
            if (hospitalTrip == null)
            {
                return false;
            }

            return Number.Equals(hospitalTrip.Number) &&
                   StartDate.Equals(hospitalTrip.StartDate) &&
                   FinishDate.Equals(hospitalTrip.FinishDate) &&
                   UserId.Equals(hospitalTrip.UserId);
        }

        public bool Equals(HospitalTrip obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Number.Equals(obj.Number) &&
                   StartDate.Equals(obj.StartDate) &&
                   FinishDate.Equals(obj.FinishDate) &&
                   UserId.Equals(obj.UserId);
        }
    }
}
