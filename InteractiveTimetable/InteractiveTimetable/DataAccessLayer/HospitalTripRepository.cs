using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    class HospitalTripRepository : BaseRepository
    {
        public HospitalTripRepository(SQLiteConnection connection) : base(connection)
        {
        }

        public HospitalTrip GetHospitalTrip(int id)
        {
            return _database.GetItem<HospitalTrip>(id);
        }

        public IEnumerable<HospitalTrip> GetHospitalTrips()
        {
            return _database.GetItems<HospitalTrip>();
        }

        public int SaveHospitalTrip(HospitalTrip hospitalTrip)
        {
            return _database.SaveItem(hospitalTrip);
        }

        public int DeleteHospitalTrip(int id)
        {
            return _database.DeleteItem<HospitalTrip>(id);
        }
    }
}
