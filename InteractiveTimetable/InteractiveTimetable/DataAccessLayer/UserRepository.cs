using System.Collections.Generic;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    internal class UserRepository : BaseRepository
    {
        internal UserRepository(SQLiteConnection connection) : base(connection)
        {
        }

        internal User GetUser(int id)
        {
            return _database.GetItemCascade<User>(id);
        }

        internal IEnumerable<User> GetUsers()
        {
            return _database.GetItemsCascade<User>();
        }

        internal int SaveUser(User user)
        {
            return _database.SaveItemCascade(user);
        }

        internal int DeleteUser(int id)
        {
            return _database.DeleteItem<User>(id);
        }

        internal void DeleteUserCascade(User user)
        {
            _database.DeleteItemCascade(user);
        }
    }
}
