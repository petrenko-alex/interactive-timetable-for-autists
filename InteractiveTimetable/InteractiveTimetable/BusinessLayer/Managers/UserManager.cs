using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.DataAccessLayer;
using InteractiveTimetable.BusinessLayer;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.BusinessLayer.Managers
{
    public class UserManager
    {
        private UserRepository _repository;

        public UserManager(SQLiteConnection connection)
        {
            _repository = new UserRepository(connection);
        }

        public User GetUser(int id)
        {
            return _repository.GetUser(id);
        }

        public IList<User> GetUsers()
        {
            return new List<User>(_repository.GetUsers());
        }

        public int SaveUser(User user)
        {
            return _repository.SaveUser(user);
        }

        public int DeleteUser(int id)
        {
            return _repository.DeleteUser(id);
        }

        public int GetAge(User user)
        {
            var today = DateTime.Today;
            var age = today.Year - user.BirthDate.Year;

            if (user.BirthDate > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}
