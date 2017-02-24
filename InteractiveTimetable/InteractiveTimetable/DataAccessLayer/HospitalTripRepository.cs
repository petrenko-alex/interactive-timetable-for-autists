using System.Collections.Generic;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    internal class HospitalTripRepository : BaseRepository
    {
        public HospitalTripRepository(SQLiteConnection connection)
            : base(connection) {}

        public HospitalTrip GetHospitalTrip(int id)
        {
            return _database.GetItem<HospitalTrip>(id);
        }

        public IEnumerable<HospitalTrip> GetHospitalTrips()
        {
            return _database.GetItems<HospitalTrip>();
        }

        public IEnumerable<HospitalTrip> GetUserHospitalTrips(int userId)
        {
            var allTrips = GetHospitalTrips();

            /* Getting all trips belonging to user and ordered by start date */
            var userTrips = allTrips.
                    Where(trip => trip.UserId == userId).
                    OrderBy(trip => trip.StartDate);

            return userTrips;
        }

        public int SaveHospitalTrip(HospitalTrip hospitalTrip)
        {
            return _database.SaveItem(hospitalTrip);
        }

        public int DeleteHospitalTrip(int id)
        {
            return _database.DeleteItem<HospitalTrip>(id);
        }

        public void DeleteHospitalTripCascade(HospitalTrip hospitalTrip)
        {
            _database.DeleteItemCascade(hospitalTrip);
        }
    }
}