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
        public int TripCount { get; private set; }

        private readonly HospitalTripRepository _repository;

        public HospitalTripManager(SQLiteConnection connection)
        {
            _repository = new HospitalTripRepository(connection);
            TripCount = _repository.GetHospitalTrips().Count();
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

            /* Save new hospital trip */
            var newHospitalTripId = _repository.SaveHospitalTrip(hospitalTrip);

            if (newHospitalTripId > 0)
            {
                TripCount++;
            }

            AdjustTripNumbers(hospitalTrip.UserId);

            return newHospitalTripId;
        }

        public void DeleteHospitalTrip(int hospitalTripId)
        {
            var hospitalTrip = GetHospitalTrip(hospitalTripId);
            if (hospitalTrip != null)
            {
                var userId = hospitalTrip.UserId;
                _repository.DeleteHospitalTripCascade(hospitalTrip);
                AdjustTripNumbers(userId);
                TripCount--;
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
            /* Checking that ... */

            /* ... hospitalTrip is set */
            if (hospitalTrip == null)
            {
                throw new ArgumentException(
                    Resources.
                    Validation.
                    HospitalTripManagerValidationStrings.
                    ArgumentIsNull);
            }

            /* ... user is set */
            if (hospitalTrip.UserId <= 0)
            {
                throw new ArgumentException(
                    Resources.
                    Validation.
                    HospitalTripManagerValidationStrings.
                    UserIsNotSet);
            }

            /* ... start date is not later than finish date */
            if (hospitalTrip.StartDate > hospitalTrip.FinishDate)
            {
                throw new ArgumentException(
                    Resources.
                    Validation.
                    HospitalTripManagerValidationStrings.
                    StartDateLaterThanFinishDate);
            }

            /* ... diagnostic is not out of trip range when editing */
            if (hospitalTrip.Id > 0)
            {
                foreach (var diagnostic in hospitalTrip.Diagnostics)
                {
                    if (diagnostic.Date < hospitalTrip.StartDate ||
                        diagnostic.Date > hospitalTrip.FinishDate)
                    {
                        var diagnosticDate =
                                diagnostic.Date.Date.ToString("dd.MM.yyyy");

                        string exceptionString = Resources.
                                Validation.
                                HospitalTripManagerValidationStrings.
                                DiagnosticOutOfTrip;

                        exceptionString = string.Format
                            (exceptionString, diagnosticDate);

                        throw new ArgumentException(exceptionString);
                    }
                }
            }

            /* ... trip is not in the past */
            var nowDateTime = DateTime.Now;
            if (hospitalTrip.StartDate < nowDateTime &&
                hospitalTrip.FinishDate < nowDateTime)
            {
                throw new ArgumentException(
                    Resources.
                    Validation.
                    HospitalTripManagerValidationStrings.
                    TripInThePast);
            }

            /* ... trip is not inside another trip */
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
                    throw new ArgumentException(
                        Resources.
                        Validation.
                        HospitalTripManagerValidationStrings.
                        TripInsideAnotherTrip);
                }
            }
        }

        private void AdjustTripNumbers(int userId)
        {
            int hospitalTripsCounter = 1;
            var userTrips = GetHospitalTrips(userId);

            foreach (var userTrip in userTrips)
            {
                userTrip.Number = hospitalTripsCounter;
                _repository.SaveHospitalTrip(userTrip);
                hospitalTripsCounter++;
            }
        }

        public void InitializeForDebugging(UserManager userManager)
        {
            var users = userManager.GetUsers();
            var randomizer = new Random();

            /* Preparing data for trips */
            var startDates = new List<DateTime>()
            {
                DateTime.Now.AddDays(-5),
                DateTime.Now.AddDays(5),
                DateTime.Now.AddDays(8),
            };

            var endDates = new List<DateTime>()
            {
                DateTime.Now.AddDays(4),
                DateTime.Now.AddDays(7),
                DateTime.Now.AddDays(10)
            };

            /* Adding trips to users */
            foreach (var user in users)
            {
                int amoutOfTrips = randomizer.Next(1,4);

                for (int i = 0; i < amoutOfTrips; ++i)
                {
                    var trip = new HospitalTrip()
                    {
                        StartDate = startDates[i],
                        FinishDate = endDates[i],
                        Number = i + 1,
                        UserId = user.Id
                    };

                    SaveHospitalTrip(trip);
                }
            }
        }
    }
}