using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Managers;
using InteractiveTimetable.BusinessLayer.Models;
using NUnit.Framework;
using SQLite;

namespace InteractiveTimetable.Tests.Managers
{
    [TestFixture]
    public class HospitalTripManagerTests
    {
        private SQLiteConnection _connection;
        private UserManager _userManager;
        private HospitalTripManager _hospitalTripManager;
        private DiagnosticManager _diagnosticManager;
        private User _user1;

        public IDictionary<string, string> GetFullSetOfCriterionsAndGrades()
        {
            var criterion18 = Resources.Repositories.CriterionDefinitionStrings.
                              Criterion18;
            Random randomizer = new Random();
            var keys = _diagnosticManager.GetCriterions().ToList();
            IDictionary<string, string> criterionAndGrades
                = new Dictionary<string, string>();

            foreach (var key in keys)
            {
                var grade = randomizer.Next(1, 5).ToString();
                if (key.Equals(criterion18))
                {
                    grade = "1010";
                }

                var pair = new KeyValuePair<string, string>(key, grade);
                criterionAndGrades.Add(pair);
            }

            return criterionAndGrades;
        }

        [SetUp]
        public void InitializationBeforeTests()
        {
            /* Creating connection */
            string dbPath = "TestsDatabase.db3";
            _connection = new SQLiteConnection(dbPath);

            /* Initializing managers */
            _userManager = new UserManager(_connection);
            _hospitalTripManager = new HospitalTripManager(_connection);
            _diagnosticManager = new DiagnosticManager(_connection);

            /* Creating users to hold hospital trips */
            _user1 = new User()
            {
                FirstName = "Alexander",
                LastName = "Petrenko",
                PatronymicName = "Andreevich",
                BirthDate = System.DateTime.Parse("25.07.1995").Date,
                PhotoPath = "avatar1.jpg",
            };
            _userManager.SaveUser(_user1);
        }

        [TearDown]
        public void ShutDown()
        {
            _connection.Dispose();
            File.Delete("TestsDatabase.db3");
        }

        [Test]
        public void CreateTripInFuture()
        {
            // arrange
            HospitalTrip newTrip = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(10),
                FinishDate = DateTime.Today.AddDays(15),
                UserId = _user1.Id
            };

            // act
            var tripId = _hospitalTripManager.SaveHospitalTrip(newTrip);
            HospitalTrip addedTrip =
                    _hospitalTripManager.GetHospitalTrip(tripId);

            // assert
            Assert.AreEqual(newTrip.StartDate, addedTrip.StartDate);
            Assert.AreEqual(newTrip.FinishDate, addedTrip.FinishDate);
            Assert.AreEqual(newTrip.UserId, addedTrip.UserId);
        }

        [Test]
        public void CreateTripInPresent()
        {
            // arrange
            HospitalTrip newTrip = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(-5),
                FinishDate = DateTime.Today.AddDays(2),
                UserId = _user1.Id
            };

            // act
            var tripId = _hospitalTripManager.SaveHospitalTrip(newTrip);
            HospitalTrip addedTrip =
                    _hospitalTripManager.GetHospitalTrip(tripId);

            // assert
            Assert.AreEqual(newTrip.StartDate, addedTrip.StartDate);
            Assert.AreEqual(newTrip.FinishDate, addedTrip.FinishDate);
            Assert.AreEqual(newTrip.UserId, addedTrip.UserId);
        }

        [Test]
        public void CreateTripWithDiagnostics()
        {
            // arrange
            /* Creating and saving hospital trip */
            HospitalTrip newTrip = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(-5),
                FinishDate = DateTime.Today.AddDays(15),
                UserId = _user1.Id
            };
            var tripId = _hospitalTripManager.SaveHospitalTrip(newTrip);

            /* Creating and saving diagnostic */
            DateTime dateTime = DateTime.Now;
            var criterionAndGrades = GetFullSetOfCriterionsAndGrades();

            // act
            var diagnosticId1 = _diagnosticManager.
                SaveDiagnostic(tripId, dateTime, criterionAndGrades);
            var diagnosticId2 = _diagnosticManager.
                SaveDiagnostic(tripId, dateTime, criterionAndGrades);

            newTrip = _hospitalTripManager.GetHospitalTrip(tripId);

            // assert
            Assert.AreEqual(2, newTrip.Diagnostics.Count);
            Assert.AreEqual(diagnosticId1, newTrip.Diagnostics[0].Id);
            Assert.AreEqual(diagnosticId2, newTrip.Diagnostics[1].Id);
        }

        [Test]
        public void CreateTripBetweenAnotherTrips()
        {
            // arrange
            HospitalTrip futureTrip = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(10),
                FinishDate = DateTime.Today.AddDays(15),
                UserId = _user1.Id
            };
           _hospitalTripManager.SaveHospitalTrip(futureTrip);


            HospitalTrip presentTrip = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(-5),
                FinishDate = DateTime.Today.AddDays(2),
                UserId = _user1.Id
            };
            _hospitalTripManager.SaveHospitalTrip(presentTrip);

            HospitalTrip newTrip = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(5),
                FinishDate = DateTime.Today.AddDays(7),
                UserId = _user1.Id
            };

            // act
            var tripId = _hospitalTripManager.SaveHospitalTrip(newTrip);

            HospitalTrip addedTrip =
                    _hospitalTripManager.GetHospitalTrip(tripId);

            futureTrip = _hospitalTripManager.GetHospitalTrip(futureTrip.Id);
            presentTrip = _hospitalTripManager.GetHospitalTrip(presentTrip.Id);

            var trips = _hospitalTripManager.GetHospitalTrips(_user1.Id);

            // assert
            Assert.AreEqual(1, presentTrip.Number);
            Assert.AreEqual(2, addedTrip.Number);
            Assert.AreEqual(3, futureTrip.Number);
        }

        [Test]
        public void CreateTripWhenUserIsNotSet()
        {
            // arrange
            HospitalTrip tripWithoutUser = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(10),
                FinishDate = DateTime.Today.AddDays(15),
            };
            string exceptionMessage = Resources.
                    Validation.
                    HospitalTripManagerValidationStrings.
                    UserIsNotSet;

            // act/assert
            var exc = Assert.Throws<ArgumentException>(
                    delegate
                    {
                        _hospitalTripManager.SaveHospitalTrip(tripWithoutUser);
                    });

            Assert.AreEqual(exceptionMessage, exc.Message);
        }

        [Test]
        public void CreateTripWithStartDateLaterThanFinishDate()
        {
            // arrange
            HospitalTrip trip = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(15),
                FinishDate = DateTime.Today.AddDays(10),
                UserId = _user1.Id
            };
            string exceptionMessage = Resources.
                    Validation.
                    HospitalTripManagerValidationStrings.
                    StartDateLaterThanFinishDate;

            // act/assert
            var exc = Assert.Throws<ArgumentException>(
                    delegate
                    {
                        _hospitalTripManager.SaveHospitalTrip(trip);
                    });

            Assert.AreEqual(exceptionMessage, exc.Message);
        }

        [Test]
        public void CreateTripInPast()
        {
            // arrange
            HospitalTrip trip = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(-20),
                FinishDate = DateTime.Today.AddDays(-10),
                UserId = _user1.Id
            };
            string exceptionMessage = Resources.
                    Validation.
                    HospitalTripManagerValidationStrings.
                    TripInThePast;

            // act/assert
            var exc = Assert.Throws<ArgumentException>(
                    delegate
                    {
                        _hospitalTripManager.SaveHospitalTrip(trip);
                    });

            Assert.AreEqual(exceptionMessage, exc.Message);
        }

        [Test]
        public void CreateTripWithIntersection()
        {
            // arrange
            HospitalTrip trip1 = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(10),
                FinishDate = DateTime.Today.AddDays(15),
                UserId = _user1.Id
            };

            HospitalTrip trip2 = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(12),
                FinishDate = DateTime.Today.AddDays(20),
                UserId = _user1.Id
            };
            string exceptionMessage = Resources.
                    Validation.
                    HospitalTripManagerValidationStrings.
                    TripInsideAnotherTrip;

            // act/assert
            _hospitalTripManager.SaveHospitalTrip(trip1);
            var exc = Assert.Throws<ArgumentException>(
                    delegate
                    {
                        _hospitalTripManager.SaveHospitalTrip(trip2);
                    });
            Assert.AreEqual(exceptionMessage, exc.Message);
        }

        [Test]
        public void CreateTripWithOverlaying()
        {
            // arrange
            HospitalTrip trip1 = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(10),
                FinishDate = DateTime.Today.AddDays(15),
                UserId = _user1.Id
            };

            HospitalTrip trip2 = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(5),
                FinishDate = DateTime.Today.AddDays(20),
                UserId = _user1.Id
            };
            string exceptionMessage = Resources.
                    Validation.
                    HospitalTripManagerValidationStrings.
                    TripInsideAnotherTrip;

            // act/assert
            _hospitalTripManager.SaveHospitalTrip(trip1);
            var exc = Assert.Throws<ArgumentException>(
                    delegate
                    {
                        _hospitalTripManager.SaveHospitalTrip(trip2);
                    });
            Assert.AreEqual(exceptionMessage, exc.Message);
        }

        [Test]
        public void EditWithoutNumberChanging()
        {
            // arrange
            HospitalTrip trip1 = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(5),
                FinishDate = DateTime.Today.AddDays(7),
                UserId = _user1.Id
            };

            HospitalTrip trip2 = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(8),
                FinishDate = DateTime.Today.AddDays(10),
                UserId = _user1.Id
            };

            _hospitalTripManager.SaveHospitalTrip(trip1);
            var trip2Id = _hospitalTripManager.SaveHospitalTrip(trip2);

            // act
            HospitalTrip addedTrip2 =
                    _hospitalTripManager.GetHospitalTrip(trip2Id);
            addedTrip2.FinishDate = DateTime.Today.AddDays(19);
            _hospitalTripManager.SaveHospitalTrip(addedTrip2);
            addedTrip2 = _hospitalTripManager.GetHospitalTrip(addedTrip2.Id);

            // assert
            Assert.AreEqual(DateTime.Today.AddDays(19), addedTrip2.FinishDate);
            Assert.AreEqual(2, addedTrip2.Number);
        }

        [Test]
        public void EditWithNumberChanging()
        {
            // arrange
            HospitalTrip trip1 = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(5),
                FinishDate = DateTime.Today.AddDays(7),
                UserId = _user1.Id
            };

            HospitalTrip trip2 = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(8),
                FinishDate = DateTime.Today.AddDays(10),
                UserId = _user1.Id
            };

            var trip1Id = _hospitalTripManager.SaveHospitalTrip(trip1);
            var trip2Id = _hospitalTripManager.SaveHospitalTrip(trip2);

            // act
            HospitalTrip addedTrip1 =
                    _hospitalTripManager.GetHospitalTrip(trip1Id);
            HospitalTrip addedTrip2 =
                    _hospitalTripManager.GetHospitalTrip(trip2Id);

            addedTrip2.StartDate = DateTime.Today.AddDays(1);
            addedTrip2.FinishDate = DateTime.Today.AddDays(3);

            _hospitalTripManager.SaveHospitalTrip(addedTrip2);
            addedTrip2 = _hospitalTripManager.GetHospitalTrip(addedTrip2.Id);
            addedTrip1 = _hospitalTripManager.GetHospitalTrip(addedTrip1.Id);


            // assert
            Assert.AreEqual(DateTime.Today.AddDays(1), addedTrip2.StartDate);
            Assert.AreEqual(DateTime.Today.AddDays(3), addedTrip2.FinishDate);
            Assert.AreEqual(1, addedTrip2.Number);
            Assert.AreEqual(2, addedTrip1.Number);
        }

        [Test]
        public void EditToIncorrectData()
        {
            // arrange
            HospitalTrip trip1 = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(5),
                FinishDate = DateTime.Today.AddDays(7),
                UserId = _user1.Id
            };
            var trip1Id = _hospitalTripManager.SaveHospitalTrip(trip1);
            string exceptionMessage = Resources.
                    Validation.
                    HospitalTripManagerValidationStrings.
                    TripInThePast;

            // act/assert
            HospitalTrip addedTrip1 =
                    _hospitalTripManager.GetHospitalTrip(trip1Id);
            
            addedTrip1.StartDate = DateTime.Today.AddDays(-5);
            addedTrip1.FinishDate = DateTime.Today.AddDays(-3);

            var exc = Assert.Throws<ArgumentException>(
                    delegate
                    {
                        _hospitalTripManager.SaveHospitalTrip(addedTrip1);
                    });
            Assert.AreEqual(exceptionMessage, exc.Message);
        }

        [Test]
        public void EditTripWithDiagnostics()
        {
            // arrange
            /* Creating and saving hospital trip */
            HospitalTrip newTrip = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(-5),
                FinishDate = DateTime.Today.AddDays(15),
                UserId = _user1.Id
            };
            var tripId = _hospitalTripManager.SaveHospitalTrip(newTrip);

            /* Creating and saving diagnostic */
            DateTime dateTime = DateTime.Now;
            var criterionAndGrades = GetFullSetOfCriterionsAndGrades();

            var diagnosticId1 = _diagnosticManager.
                SaveDiagnostic(tripId, dateTime, criterionAndGrades);
            var diagnosticId2 = _diagnosticManager.
                SaveDiagnostic(tripId, dateTime, criterionAndGrades);


            // act
            _diagnosticManager.DeleteDiagnostic(diagnosticId1);
            var diagnosticId3 = _diagnosticManager.
                SaveDiagnostic(tripId, dateTime, criterionAndGrades);
            var diagnosticId4 = _diagnosticManager.
                            SaveDiagnostic(tripId, dateTime, criterionAndGrades);

            newTrip = _hospitalTripManager.GetHospitalTrip(tripId);

            // assert
            Assert.AreEqual(3, newTrip.Diagnostics.Count);
            Assert.AreEqual(diagnosticId2, newTrip.Diagnostics[0].Id);
            Assert.AreEqual(diagnosticId3, newTrip.Diagnostics[1].Id);
            Assert.AreEqual(diagnosticId4, newTrip.Diagnostics[2].Id);
        }

        [Test]
        public void EditToMakeDiagnosticOutOfTripRange()
        {
            // arrange
            DateTime dateTime = DateTime.Now;
            var exceptionString = Resources.
                    Validation.
                    HospitalTripManagerValidationStrings.
                    DiagnosticOutOfTrip;
            exceptionString = string.Format(
                exceptionString, 
                dateTime.Date.ToString("dd.MM.yyyy"));

            /* Creating and saving hospital trip */
            HospitalTrip newTrip = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(-5),
                FinishDate = DateTime.Today.AddDays(15),
                UserId = _user1.Id
            };
            var tripId = _hospitalTripManager.SaveHospitalTrip(newTrip);

            /* Creating and saving diagnostic */
            var criterionAndGrades = GetFullSetOfCriterionsAndGrades();

            var diagnosticId1 = _diagnosticManager.
                SaveDiagnostic(tripId, dateTime, criterionAndGrades);
            var diagnosticId2 = _diagnosticManager.
                SaveDiagnostic(tripId, dateTime, criterionAndGrades);

            // act/assert
            var trip = _hospitalTripManager.GetHospitalTrip(tripId);
            trip.StartDate = DateTime.Today.AddDays(5);

            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _hospitalTripManager.SaveHospitalTrip(trip);
            });
            Assert.AreEqual(exceptionString, exception.Message);

        }

        [Test]
        public void DeleteWithoutNumberChanging()
        {
            // arrange
            HospitalTrip trip1 = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(5),
                FinishDate = DateTime.Today.AddDays(7),
                UserId = _user1.Id
            };

            HospitalTrip trip2 = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(8),
                FinishDate = DateTime.Today.AddDays(10),
                UserId = _user1.Id
            };

            HospitalTrip trip3 = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(11),
                FinishDate = DateTime.Today.AddDays(15),
                UserId = _user1.Id
            };

            var trip1Id = _hospitalTripManager.SaveHospitalTrip(trip1);
            var trip2Id = _hospitalTripManager.SaveHospitalTrip(trip2);
            var trip3Id = _hospitalTripManager.SaveHospitalTrip(trip3);


            // act
            _hospitalTripManager.DeleteHospitalTrip(trip3Id);
            var deletedTrip = _hospitalTripManager.GetHospitalTrip(trip3Id);
            var addedTrip1 = _hospitalTripManager.GetHospitalTrip(trip1Id);
            var addedTrip2 = _hospitalTripManager.GetHospitalTrip(trip2Id);

            // assert
            Assert.AreEqual(1, addedTrip1.Number);
            Assert.AreEqual(2, addedTrip2.Number);
            Assert.AreEqual(null, deletedTrip);
        }

        [Test]
        public void DeleteWithNumberChanging()
        {
            // arrange
            HospitalTrip trip1 = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(5),
                FinishDate = DateTime.Today.AddDays(7),
                UserId = _user1.Id
            };

            HospitalTrip trip2 = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(8),
                FinishDate = DateTime.Today.AddDays(10),
                UserId = _user1.Id
            };

            HospitalTrip trip3 = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(11),
                FinishDate = DateTime.Today.AddDays(15),
                UserId = _user1.Id
            };

            var trip1Id = _hospitalTripManager.SaveHospitalTrip(trip1);
            var trip2Id = _hospitalTripManager.SaveHospitalTrip(trip2);
            var trip3Id = _hospitalTripManager.SaveHospitalTrip(trip3);

            // act
            _hospitalTripManager.DeleteHospitalTrip(trip1Id);
            var deletedTrip = _hospitalTripManager.GetHospitalTrip(trip1Id);
            var addedTrip2 = _hospitalTripManager.GetHospitalTrip(trip2Id);
            var addedTrip3 = _hospitalTripManager.GetHospitalTrip(trip3Id);

            // assert
            Assert.AreEqual(1, addedTrip2.Number);
            Assert.AreEqual(2, addedTrip3.Number);
            Assert.AreEqual(null, deletedTrip);
        }

        [Test]
        public void PresentHospitalTrip()
        {
            // arrange
            HospitalTrip trip1 = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(-2),
                FinishDate = DateTime.Today.AddDays(4),
                UserId = _user1.Id
            };
            var tripId = _hospitalTripManager.SaveHospitalTrip(trip1);

            // act
            var IsTripPresent =
                    _hospitalTripManager.IsHospitalTripPresent(tripId);

            // assert
            Assert.AreEqual(true, IsTripPresent);
        }

        [Test]
        public void NotPresentHospitalTrip()
        {
            // arrange
            HospitalTrip trip1 = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(2),
                FinishDate = DateTime.Today.AddDays(4),
                UserId = _user1.Id
            };
            var tripId = _hospitalTripManager.SaveHospitalTrip(trip1);

            // act
            var IsTripPresent =
                    _hospitalTripManager.IsHospitalTripPresent(tripId);

            // assert
            Assert.AreEqual(false, IsTripPresent);

        }

        [Test]
        public void DeleteTripWithDiagnostics()
        {
            // arrange
            /* Creating and saving hospital trip */
            HospitalTrip newTrip = new HospitalTrip()
            {
                StartDate = DateTime.Today.AddDays(-5),
                FinishDate = DateTime.Today.AddDays(15),
                UserId = _user1.Id
            };
            var tripId = _hospitalTripManager.SaveHospitalTrip(newTrip);

            /* Creating and saving diagnostic */
            DateTime dateTime = DateTime.Now;
            var criterionAndGrades = GetFullSetOfCriterionsAndGrades();

            // act
            var diagnosticId1 = _diagnosticManager.
                SaveDiagnostic(tripId, dateTime, criterionAndGrades);
            var diagnosticId2 = _diagnosticManager.
                SaveDiagnostic(tripId, dateTime, criterionAndGrades);

            // act
            _hospitalTripManager.DeleteHospitalTrip(tripId);
            var deletedTrip = _hospitalTripManager.GetHospitalTrip(tripId);
            var deletedDiagnostic1 =
                    _diagnosticManager.GetDiagnostic(diagnosticId1);
            var deletedDiagnostic2 =
                    _diagnosticManager.GetDiagnostic(diagnosticId2);

            // assert
            Assert.AreEqual(null, deletedTrip);
            Assert.AreEqual(null, deletedDiagnostic1);
            Assert.AreEqual(null, deletedDiagnostic2);
        }
    }
}