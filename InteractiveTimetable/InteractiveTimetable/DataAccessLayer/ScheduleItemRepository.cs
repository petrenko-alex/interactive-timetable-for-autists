using System;
using System.Collections.Generic;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    internal class ScheduleItemRepository : BaseRepository
    {
        internal ScheduleItemRepository(SQLiteConnection connection) : base(connection)
        {
        }

        internal ScheduleItem GetScheduleItem(int itemId)
        {
            return _database.GetItem<ScheduleItem>(itemId);
        }

        internal IEnumerable<ScheduleItem> GetScheduleItems()
        {
            return _database.GetItems<ScheduleItem>();
        }

        internal IEnumerable<ScheduleItem> GetScheduleItemsOfSchedule(
            int scheduleId)
        {
            var allScheduleItems = GetScheduleItems();

            /*
             * Getting schedule items belonging to schedule 
             * and ordered by order number
             */ 
            var scheduleItemsOfSchedule = allScheduleItems.
                    Where(x => x.ScheduleId == scheduleId).
                    OrderBy(x => x.OrderNumber);

            return scheduleItemsOfSchedule;
        }

        internal int SaveScheduleItem(ScheduleItem item)
        {
            /* Data validation */
            Validate(item);

            return _database.SaveItem(item);
        }

        internal int DeleteScheduleItem(int itemId)
        {
            return _database.DeleteItem<ScheduleItem>(itemId);
        }

        internal void Validate(ScheduleItem item)
        {
            /* Checking that ... */

            /* ... item is set */
            if (item == null)
            {
                throw new ArgumentException(Resources.Validation.
                                                      ScheduleItemValidationStrings.
                                                      ArgumentIsNull);
            }

            /* ... valid order number is set */
            if (item.OrderNumber <= 0)
            {
                throw new ArgumentException(Resources.Validation.
                                                      ScheduleItemValidationStrings.
                                                      NotCorrectOrderNumber);
            }
        }
    }
}
