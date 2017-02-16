using System;
using System.Collections.Generic;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.DataAccessLayer;
using SQLite;

namespace InteractiveTimetable.BusinessLayer.Managers
{
    public class HospitalTripManager
    {
        private readonly HospitalTripRepository _repository;

        public HospitalTripManager(SQLiteConnection connection)
        {
            _repository = new HospitalTripRepository(connection);
        }

        public HospitalTrip GetHospitalTrip(int hospitalTripId)
        {
            return _repository.GetHospitalTrip(hospitalTripId);
        }

        public IEnumerable<HospitalTrip> GetHospitalTrips(int userId)
        {
            return _repository.GetUserHospitalTrips(userId);
        }

        public int SaveHospitalTrip(HospitalTrip hospitalTrip)
        {
            /* Data validation */
            Validate(hospitalTrip);

            /* Adjust trip numbers */
            int hospitalTripsCounter = 1;
            int numberForTheNewTrip = 1;
            bool isNumberForTheNewTripSet = false;
            var userTrips = GetHospitalTrips(hospitalTrip.UserId);

            foreach (var userTrip in userTrips)
            {
                if (hospitalTrip.StartDate < userTrip.StartDate &&
                    !isNumberForTheNewTripSet)
                {
                    numberForTheNewTrip = hospitalTripsCounter;
                    hospitalTripsCounter++;
                    isNumberForTheNewTripSet = true;
                }

                userTrip.Number = hospitalTripsCounter;
                _repository.SaveHospitalTrip(userTrip);
                hospitalTripsCounter++;
            }

            /* Save new hospital trip */
            hospitalTrip.Number = numberForTheNewTrip;
            return _repository.SaveHospitalTrip(hospitalTrip);
        }

        public void DeleteHospitalTrip(int hospitalTripId)
        {
            /* Delete hospital trip */
            var hospitalTrip = GetHospitalTrip(hospitalTripId);
            _repository.CascadeDelete(hospitalTrip);

            /* Adjust trip numbers */
            int hospitalTripsCounter = 1;
            var userTrips = GetHospitalTrips(hospitalTrip.UserId);

            foreach (var userTrip in userTrips)
            {
                userTrip.Number = hospitalTripsCounter;
                _repository.SaveHospitalTrip(userTrip);
                hospitalTripsCounter++;
            }
        }

        public bool IsHospitalTripPresent(int hospitalTripId)
        {
            var hospitalTrip = GetHospitalTrip(hospitalTripId);
            var nowDateTime = DateTime.Now;

            return nowDateTime > hospitalTrip.StartDate &&
                   nowDateTime < hospitalTrip.FinishDate;
        }

        private void Validate(HospitalTrip hospitalTrip)
        {
            /* User is not set */
            if (hospitalTrip.UserId <= 0)
            {
                throw new ArgumentException("User id is not set.");
            }

            /* Start date is later than finish date */
            if (hospitalTrip.StartDate > hospitalTrip.FinishDate)
            {
                throw new ArgumentException("The start date of a " +
                                            "hospital trip can't be later " +
                                            "or equal than the finish date.");
            }

            /* TODO Dianostic out of trip range */

            /* Trip in the past */
            var nowDateTime = DateTime.Now;
            if (hospitalTrip.StartDate < nowDateTime &&
                hospitalTrip.FinishDate < nowDateTime)
            {
                throw new ArgumentException("Trip in the past is not allowed");
            }

            /* Trip inside another trip */
            var userTrips = GetHospitalTrips(hospitalTrip.UserId);
            foreach (var userTrip in userTrips)
            {
                bool newTripIntersectsAnotherTrip = hospitalTrip.StartDate >
                                                    userTrip.StartDate &&
                                                    hospitalTrip.StartDate <
                                                    userTrip.FinishDate ||
                                                    hospitalTrip.FinishDate >
                                                    userTrip.StartDate &&
                                                    hospitalTrip.FinishDate <
                                                    userTrip.FinishDate;

                bool newTripOveraysAnotherTrip = userTrip.StartDate >
                                                 hospitalTrip.StartDate &&
                                                 userTrip.StartDate <
                                                 hospitalTrip.FinishDate &&
                                                 userTrip.FinishDate >
                                                 hospitalTrip.StartDate &&
                                                 userTrip.FinishDate <
                                                 hospitalTrip.FinishDate;

                if (newTripOveraysAnotherTrip || newTripIntersectsAnotherTrip)
                {
                    throw new ArgumentException("Date period is already " +
                                                "occupied by another trip");
                }
            }
        }
    }
}