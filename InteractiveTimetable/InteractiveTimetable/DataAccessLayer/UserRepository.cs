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
    class UserRepository
    {
        private Database _database = null;

        public UserRepository(SQLiteConnection connection)
        {
            _database = new Database(connection);
        }

        public User GetUser(int id)
        {
            return _database.GetItem<User>(id);
        }

        public IEnumerable<User> GetUsers()
        {
            return _database.GetItems<User>();
        }

        public int SaveUser(User user)
        {
            return _database.SaveItem(user);
        }

        public int DeleteUser(int id)
        {
            return _database.DeleteItem<User>(id);
        }
    }
}
