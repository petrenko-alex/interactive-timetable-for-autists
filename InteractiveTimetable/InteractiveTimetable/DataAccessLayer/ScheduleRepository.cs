using System.Collections.Generic;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    internal class ScheduleRepository : BaseRepository
    {
        internal ScheduleRepository(SQLiteConnection connection) : base(connection)
        {
        }

        internal Schedule GetSchedule(int scheduleId)
        {
            return _database.GetItemCascade<Schedule>(scheduleId);
        }

        internal IEnumerable<Schedule> GetSchedules()
        {
            return _database.GetItemsCascade<Schedule>();
        }

        internal IEnumerable<Schedule> GetUserSchedules(int userId)
        {
            var allSchedules = GetSchedules();

            /*
             * Getting all schedules belonging to user and 
             * ordered by creation time
             */ 
            var userSchedules = allSchedules.
                    Where(x => x.UserId == userId).
                    OrderBy(x => x.CreateTime);

            return userSchedules;
        }

        internal int SaveSchedule(Schedule schedule)
        {
            return _database.SaveItemCascade(schedule);
        }

        internal int DeleteSchedule(int scheduleId)
        {
            return _database.DeleteItem<Schedule>(scheduleId);
        }

        internal void DeleteScheduleCascade(Schedule schedule)
        {
            _database.DeleteItemCascade(schedule);
        }
    }
}
