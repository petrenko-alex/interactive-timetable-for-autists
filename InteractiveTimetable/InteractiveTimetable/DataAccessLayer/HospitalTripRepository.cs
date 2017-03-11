using System.Collections.Generic;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    internal class HospitalTripRepository : BaseRepository
    {
        internal HospitalTripRepository(SQLiteConnection connection)
            : base(connection) {}

        internal HospitalTrip GetHospitalTrip(int id)
        {
            return _database.GetItemCascade<HospitalTrip>(id);
        }

        internal IEnumerable<HospitalTrip> GetHospitalTrips()
        {
            return _database.GetItemsCascade<HospitalTrip>();
        }

        internal IEnumerable<HospitalTrip> GetUserHospitalTrips(int userId)
        {
            var allTrips = GetHospitalTrips();

            /* Getting all trips belonging to user and ordered by start date */
            var userTrips = allTrips.
                    Where(trip => trip.UserId == userId).
                    OrderBy(trip => trip.StartDate);

            return userTrips;
        }

        internal int SaveHospitalTrip(HospitalTrip hospitalTrip)
        {
            return _database.SaveItemCascade(hospitalTrip);
        }

        internal int DeleteHospitalTrip(int id)
        {
            return _database.DeleteItem<HospitalTrip>(id);
        }

        internal void DeleteHospitalTripCascade(HospitalTrip hospitalTrip)
        {
            _database.DeleteItemCascade(hospitalTrip);
        }
    }
}