using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.BusinessLayer.Managers;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.Tests
{
    [TestFixture]
    public class HospitalTripManagerTests
    {
        private SQLiteConnection _connection;
        private UserManager _userManager;
        private HospitalTripManager _hospitalTripManager;
        private User _user1;

        [SetUp]
        public void InitializationBeforeTests()
        {
            /* Creating connection */
            string dbPath = "TestsDatabase.db3";
            _connection = new SQLiteConnection(dbPath);

            /* Initializing managers */
            _userManager = new UserManager(_connection);
            _hospitalTripManager = new HospitalTripManager(_connection);

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
            Assert.AreEqual(newTrip, addedTrip);
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
            Assert.AreEqual(newTrip, addedTrip);
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
            string exceptionMessage = "User id is not set.";

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
            string exceptionMessage = "The start date of a " +
                                      "hospital trip can't be later " +
                                      "or equal than the finish date.";

            // act/assert
            var exc = Assert.Throws<ArgumentException>(
                    delegate
                    {
                        _hospitalTripManager.SaveHospitalTrip(trip);
                    });

            Assert.AreEqual(exceptionMessage, exc.Message);
        }
    }
}