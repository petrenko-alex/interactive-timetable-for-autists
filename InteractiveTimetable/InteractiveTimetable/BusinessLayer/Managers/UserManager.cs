using System;
using System.Collections.Generic;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.DataAccessLayer;
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

        public User GetUser(int userId)
        {
            return _repository.GetUser(userId);
        }

        public IEnumerable<User> GetUsers()
        {
            return _repository.GetUsers();
        }

        public int SaveUser(User user)
        {
            /* Data validation */
            Validate(user);

            return _repository.SaveUser(user);
        }

        public void DeleteUser(int userId)
        {
            var user = GetUser(userId);
            _repository.DeleteUserCascade(user);
        }

        public bool IsUserInPresentTimetable(int userId)
        {
            var user = _repository.GetUser(userId);
            var nowDateTime = DateTime.Now;

            var userHasCurrentTrip = user.
                    HospitalTrips.
                    Any(trip => trip.StartDate <= nowDateTime &&
                                  trip.FinishDate >= nowDateTime);

            return userHasCurrentTrip;
        }

        private void Validate(User user)
        {
            // TODO: Check that only symbols?
            // TODO: Check SQL injections?

            /* Checking that ... */

            /* ... user is set */
            if (user == null)
            {
                throw new ArgumentException("User is not set.");
            }

            /* ... strings are not longer than 255 symbols */
            if (user.FirstName.Length > 255)
            {
                throw new ArgumentException("The length of a user first name " +
                                            "must be less than 255 symbols.");
            }

            if (user.LastName.Length > 255)
            {
                throw new ArgumentException("The length of a user last name " +
                                            "must be less than 255 symbols.");
            }

            if (user.PatronymicName.Length > 255)
            {
                throw new ArgumentException("The length of a user patronymic " +
                                            "name must be less than 255 symbols.");
            }

            /* ... path to photo is not longer than 1024 symbols */
            if (user.PhotoPath.Length > 1024)
            {
                throw new ArgumentException("The length of the path " +
                                            "to the user photo must be less " +
                                            "than 1024 symbols");
            }

            /* ... birth date is not later than current date or earlier than 1990 */
            if (user.BirthDate.Date < DateTime.Parse("01.01.1900").Date ||
                user.BirthDate.Date > DateTime.Today.Date)
            {
                throw new ArgumentException("The birth date is " +
                                            "set not correctly");
            }

            /* ... birth date is not later than trip start date (when edit user) */
            if (user.Id > 0 && user.HospitalTrips.Any())
            {
                var firstTrip = user.
                        HospitalTrips.
                        OrderBy(trip => trip.StartDate).
                        First();

                if (user.BirthDate.Date > firstTrip.StartDate)
                {
                    throw new ArgumentException("The user birth date can not " +
                                                "be later than his hospital " +
                                                "trips");
                }
            }
        }
    }
}