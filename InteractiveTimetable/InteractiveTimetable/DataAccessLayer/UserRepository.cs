using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.DataLayer;
using InteractiveTimetable.BusinessLayer;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    class UserRepository : BaseRepository
    {
        public UserRepository(SQLiteConnection connection) : base(connection)
        {
        }

        public User GetUser(int id)
        {
            return _database.GetItemCascade<User>(id);
        }

        public IEnumerable<User> GetUsers()
        {
            return _database.GetItemsCascade<User>();
        }

        public int SaveUser(User user)
        {
            return _database.SaveItemCascade(user);
        }

        public int DeleteUser(int id)
        {
            return _database.DeleteItem<User>(id);
        }

        public void DeleteUserCascade(User user)
        {
            _database.DeleteItemCascade(user);
        }
    }
}
