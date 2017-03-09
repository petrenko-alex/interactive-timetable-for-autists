using InteractiveTimetable.DataLayer;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    public abstract class BaseRepository
    {
        internal Database _database = null;

        protected BaseRepository(SQLiteConnection connection)
        {
            _database = new Database(connection);
        }
    }
}
