using System;
using System.Collections.Generic;
using System.Globalization;
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

            if (user.FirstName.Length == 0)
            {
                throw new ArgumentException(Resources.Validation.
                                                      UserValidationStrings.
                                                      FirstNameIsNotSet);
            }

            if (user.LastName.Length > 255)
            {
                throw new ArgumentException(Resources.Validation.
                                                      UserValidationStrings.
                                                      LastNameLength);
            }

            if (user.LastName.Length == 0)
            {
                throw new ArgumentException(Resources.Validation.
                                                      UserValidationStrings.
                                                      LastNameIsNotSet);
            }

            if (user.PatronymicName.Length > 255)
            {
                throw new ArgumentException(Resources.Validation.
                                                      UserValidationStrings.
                                                      PatronymicNameLength);
            }

            if (user.PatronymicName.Length == 0)
            {
                throw new ArgumentException(Resources.Validation.
                                                      UserValidationStrings.
                                                      PatronymicNameIsNotSet);
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

        public void InitializeForDebugging(string appFolderPath)
        {
            // TODO: Delete method calls when debugging is not needed
            User user1 = new User()
            {
                LastName = "Петренко",
                FirstName = "Александр",
                PatronymicName = "Андреевич",
                BirthDate = DateTime.ParseExact("25.07.1995", "dd.MM.yyyy", CultureInfo.CurrentCulture).Date,
                PhotoPath = appFolderPath + "/user1.jpg"
            };

            User user2 = new User()
            {
                LastName = "Шкулипа",
                FirstName = "Дмитрий",
                PatronymicName = "Алексеевич",
                BirthDate = DateTime.ParseExact("16.03.1994", "dd.MM.yyyy", CultureInfo.CurrentCulture).Date,
                PhotoPath = appFolderPath + "/user2.jpg"
            };

            User user3 = new User()
            {
                LastName = "Иванов",
                FirstName = "Алексей",
                PatronymicName = "Борисович",
                BirthDate = DateTime.ParseExact("11.02.1999", "dd.MM.yyyy", CultureInfo.CurrentCulture).Date,
                PhotoPath = appFolderPath + "/user3.jpg"
            };

            User user4 = new User()
            {
                LastName = "Апарин",
                FirstName = "Дмитрий",
                PatronymicName = "Сергеевич",
                BirthDate = DateTime.ParseExact("20.01.1995", "dd.MM.yyyy", CultureInfo.CurrentCulture).Date,
                PhotoPath = appFolderPath + "/user4.jpg"
            };

            User user5 = new User()
            {
                LastName = "Краснов",
                FirstName = "Иван",
                PatronymicName = "Викторович",
                BirthDate = DateTime.ParseExact("01.01.1990", "dd.MM.yyyy", CultureInfo.CurrentCulture).Date,
                PhotoPath = appFolderPath + "/user5.jpg"
            };

            User user6 = new User()
            {
                LastName = "Тополев",
                FirstName = "Александр",
                PatronymicName = "Владимирович",
                BirthDate = DateTime.ParseExact("05.05.1992", "dd.MM.yyyy", CultureInfo.CurrentCulture).Date,
                PhotoPath = appFolderPath + "/user6.jpg"
            };

            User user7 = new User()
            {
                LastName = "Бивченко",
                FirstName = "Александр",
                PatronymicName = "Александрович",
                BirthDate = DateTime.ParseExact("22.12.1990", "dd.MM.yyyy", CultureInfo.CurrentCulture).Date,
                PhotoPath = appFolderPath + "/user7.jpg"
            };

            User user8 = new User()
            {
                LastName = "Прокопенко",
                FirstName = "Юрий",
                PatronymicName = "Викторович",
                BirthDate = DateTime.ParseExact("01.01.2001", "dd.MM.yyyy", CultureInfo.CurrentCulture).Date,
                PhotoPath = appFolderPath + "/user8.jpg"
            };

            User user9 = new User()
            {
                LastName = "Агаров",
                FirstName = "Михаил",
                PatronymicName = "Константинович",
                BirthDate = DateTime.ParseExact("03.06.1988", "dd.MM.yyyy", CultureInfo.CurrentCulture).Date,
                PhotoPath = appFolderPath + "/user9.jpg"
            };

            User user10 = new User()
            {
                LastName = "Русов",
                FirstName = "Андрей",
                PatronymicName = "Борисович",
                BirthDate = DateTime.ParseExact("30.11.2005", "dd.MM.yyyy", CultureInfo.CurrentCulture).Date,
                PhotoPath = appFolderPath + "/user10.jpg"
            };

            SaveUser(user1);
            SaveUser(user2);
            SaveUser(user3);
            SaveUser(user4);
            SaveUser(user5);
            SaveUser(user6);
            SaveUser(user7);
            SaveUser(user8);
            SaveUser(user9);
            SaveUser(user10);
        }
    }
}