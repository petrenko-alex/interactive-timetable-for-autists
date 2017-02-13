using System;
using System.Collections.Generic;
using InteractiveTimetable.BusinessLayer.Contracts;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace InteractiveTimetable.BusinessLayer.Models
{
    public class User : BusinessEntityBase
    {
        [MaxLength(255), NotNull]
        public string FirstName { get; set; }

        [MaxLength(255), NotNull]
        public string LastName { get; set; }

        [MaxLength(255), NotNull]
        public string PatronymicName { get; set; }

        // TODO: Change to String?
        [NotNull]
        public DateTime BirthDate { get; set; }

        [MaxLength(1024), NotNull]
        public string PhotoPath { get; set; }

        [NotNull]
        public bool IsDeleted { get; set; } = false;

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<HospitalTrip> HospitalTrips { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<Schedule> Schedules { get; set; }
    }
}
