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

        public override bool Equals(object obj)
        {
            /* If obj is null return false */
            if (obj == null)
            {
                return false;
            }

            /* If obj can not be cast to class type return false*/
            HospitalTrip hospitalTrip = obj as HospitalTrip;
            if ((System.Object) hospitalTrip == null)
            {
                return false;
            }

            return Number.Equals(hospitalTrip.Number) &&
                   StartDate.Equals(hospitalTrip.StartDate) &&
                   FinishDate.Equals(hospitalTrip.FinishDate) &&
                   UserId.Equals(hospitalTrip.UserId);
        }
    }
}
