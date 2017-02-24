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

        [NotNull]
        public DateTime BirthDate { get; set; }

        [MaxLength(1024), NotNull]
        public string PhotoPath { get; set; }

        [NotNull]
        public bool IsDeleted { get; set; } = false;

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<HospitalTrip> HospitalTrips { get; set; } 
            = new List<HospitalTrip>();

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<Schedule> Schedules { get; set; } 
            = new List<Schedule>();

        [Ignore]
        public int Age
        {
            get
            {
                if (_age >= 0)
                {
                    return _age;
                }

                /* If age is not set yet */
                var today = DateTime.Today;
                _age = today.Year - BirthDate.Year;

                if (BirthDate > today.AddYears(-_age))
                {
                    _age--;
                }

                return _age;
            }
        }

        private int _age = -1;
    }
}