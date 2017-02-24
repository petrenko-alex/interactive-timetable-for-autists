using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.DataLayer;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    public abstract class BaseRepository
    {
        protected Database _database = null;

        public BaseRepository(SQLiteConnection connection)
        {
            _database = new Database(connection);
        }
    }
}
