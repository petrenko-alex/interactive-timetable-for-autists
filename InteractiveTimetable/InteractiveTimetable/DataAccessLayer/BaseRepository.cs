using InteractiveTimetable.DataLayer;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    internal abstract class BaseRepository
    {
        internal Database _database = null;

        protected BaseRepository(SQLiteConnection connection)
        {
            _database = new Database(connection);
        }
    }
}
