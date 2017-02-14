using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    class ScheduleItemRepository : BaseRepository
    {
        public ScheduleItemRepository(SQLiteConnection connection) : base(connection)
        {
        }

        public ScheduleItem GetScheduleItem(int id)
        {
            return _database.GetItem<ScheduleItem>(id);
        }

        public IEnumerable<ScheduleItem> GetScheduleItems()
        {
            return _database.GetItems<ScheduleItem>();
        }

        public int SaveScheduleItem(ScheduleItem scheduleItem)
        {
            return _database.SaveItem(scheduleItem);
        }

        public int DeleteScheduleItem(int id)
        {
            return _database.DeleteItem<ScheduleItem>(id);
        }

    }
}
