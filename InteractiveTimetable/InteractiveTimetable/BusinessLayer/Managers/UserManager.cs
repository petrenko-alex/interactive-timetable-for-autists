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
        public int UserCount { get; private set; }

        private UserRepository _repository;

        public UserManager(SQLiteConnection connection)
        {
            _repository = new UserRepository(connection);
            UserCount = _repository.GetUsers().Count();
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

            /* Saving user */
            var savedId = _repository.SaveUser(user);

            if (savedId > 0)
            {
                UserCount++;
            }

            return savedId;
        }

        public void DeleteUser(int userId)
        {
            var user = GetUser(userId);
            if (user != null)
            {
                _repository.DeleteUserCascade(user);
                UserCount--;
            }
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
                throw new ArgumentException(Resources.Validation.
                                                      UserValidationStrings.
                                                      ArgumentIsNull);
            }

            /* ... strings are not longer than 255 symbols */
            if (user.FirstName.Length > 255)
            {
                throw new ArgumentException(Resources.Validation.
                                                      UserValidationStrings.
                                                      FirstNameLength);
            }

            if (user.LastName.Length > 255)
            {
                throw new ArgumentException(Resources.Validation.
                                                      UserValidationStrings.
                                                      LastNameLength);
            }

            if (user.PatronymicName.Length > 255)
            {
                throw new ArgumentException(Resources.Validation.
                                                      UserValidationStrings.
                                                      PatronymicNameLength);
            }

            /* ... path to photo is not longer than 1024 symbols */
            if (user.PhotoPath.Length > 1024)
            {
                throw new ArgumentException(Resources.Validation.
                                                      UserValidationStrings.
                                                      PhotoPathLength);
            }

            /* ... birth date is not later than current date or earlier than 1990 */
            if (user.BirthDate.Date < DateTime.Parse("01.01.1900").Date ||
                user.BirthDate.Date > DateTime.Today.Date)
            {
                throw new ArgumentException(Resources.Validation.
                                                      UserValidationStrings.
                                                      NotCorrectBirthDate);
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
                    throw new ArgumentException(Resources.Validation.
                                                          UserValidationStrings.
                                                          BirthDateLaterThanTrip);
                }
            }
        }
    }
}