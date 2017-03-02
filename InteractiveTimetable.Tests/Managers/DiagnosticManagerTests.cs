using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.BusinessLayer.Managers;
using InteractiveTimetable.BusinessLayer.Models;
using NUnit.Framework;
using SQLite;

namespace InteractiveTimetable.Tests.Managers
{
    [TestFixture]
    class DiagnosticManagerTests
    {
        private SQLiteConnection _connection;
        private DiagnosticManager _diagnosticManager;
        private UserManager _userManager;
        private HospitalTripManager _hospitalTripManager;
        private User _user;
        private HospitalTrip _trip;

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

        public IDictionary<string, string> GetNotFullSetOfCriterionsAndGrades()
        {
            Random randomizer = new Random();
            var keys = _diagnosticManager.GetCriterions().ToList();
            keys.RemoveRange(9, 9);
            IDictionary<string, string> criterionAndGrades
                = new Dictionary<string, string>();

            foreach (var key in keys)
            {
                var grade = randomizer.Next(1, 5).ToString();
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
            _diagnosticManager = new DiagnosticManager(_connection);
            _userManager = new UserManager(_connection);
            _hospitalTripManager = new HospitalTripManager(_connection);

            /* Creating a user and a hospital trip */
            _user = new User()
            {
                FirstName = "Alexander",
                LastName = "Petrenko",
                PatronymicName = "Andreevich",
                BirthDate = System.DateTime.Parse("25.07.1995").Date,
                PhotoPath = "avatar1.jpg",
            };
            var userId = _userManager.SaveUser(_user);
            _user = _userManager.GetUser(userId);

            _trip = new HospitalTrip()
            {
                StartDate = DateTime.Now.AddDays(-10),
                FinishDate = DateTime.Now.AddDays(10),
                UserId = _user.Id
            };
            var tripId = _hospitalTripManager.SaveHospitalTrip(_trip);
            _trip = _hospitalTripManager.GetHospitalTrip(tripId);

        }

        [TearDown]
        public void ShutDown()
        {
            _connection.Dispose();
            File.Delete("TestsDatabase.db3");
        }

        [Test]
        public void GetNumberOfCriterions()
        {
            // arrange
            var expectedNumber = 18;

            // act
            var actualNumber = _diagnosticManager.GetNumberOfCriterions();

            // assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }

        [Test]
        public void GetCriterions()
        {
            // arrange
            var numberOfCriterions = _diagnosticManager.GetNumberOfCriterions();
            var definitions = new List<string>();
            
            for (int i = 1; i <= numberOfCriterions; ++i)
            {
                var resourceString = "Criterion" + i;
                var definition = Resources.
                    Repositories.
                    CriterionDefinitionStrings.
                    ResourceManager.
                    GetString(resourceString);

                definitions.Add(definition);
            }

            // act
            var actualDefinitions = _diagnosticManager.GetCriterions();

            // assert
            Assert.AreEqual(definitions, actualDefinitions);
        }

        [Test]
        public void SaveDiagnosticWithFullSetOfCriterions()
        {
            // arrange
            DateTime dateTime = DateTime.Now;
            var criterionAndGrades = GetFullSetOfCriterionsAndGrades();

            // act
            var diagnosticId = _diagnosticManager.
                SaveDiagnostic(_trip.Id, dateTime, criterionAndGrades);
            var diagnostic = _diagnosticManager.GetDiagnostic(diagnosticId);
            var trip = _hospitalTripManager.GetHospitalTrip(_trip.Id);

            // assert
            Assert.AreEqual(criterionAndGrades.Count,
                            diagnostic.CriterionGrades.Count);
            Assert.AreEqual(dateTime, diagnostic.Date);
        }

        [Test]
        public void SaveDiagnosticWithNotFullSetOfCriterions()
        {
            // arrange
            DateTime dateTime = DateTime.Now;
            var criterionAndGrades = GetNotFullSetOfCriterionsAndGrades();
            var exceptionString = "Not all criterions and grades " +
                                  "are present";

            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _diagnosticManager.SaveDiagnostic(_trip.Id, dateTime,
                                                  criterionAndGrades);
            });
            Assert.AreEqual(exceptionString, exception.Message);
        }

        [Test]
        public void SaveDiagnosticWithEmptySetOfCriterions()
        {
            // arrange
            DateTime dateTime = DateTime.Now;
            IDictionary<string, string> criterionAndGrades
                = new Dictionary<string, string>();
            string exceptionMessage = "Criterions and grades " +
                                      "are not present.";

            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _diagnosticManager.SaveDiagnostic(_trip.Id, dateTime,
                                                  criterionAndGrades);
            });
            Assert.AreEqual(exceptionMessage, exception.Message);
        }

        [Test]
        public void SaveDiagnosticWithIncorrectDate()
        {
            // arrange
            DateTime dateTime = DateTime.Now.AddDays(100);
            var criterionAndGrades = GetFullSetOfCriterionsAndGrades();
            var exceptrionString = "Diagnostic date can not be " +
                                   "outside of the current " +
                                   "hospital trip time bounds.";

            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _diagnosticManager.SaveDiagnostic(_trip.Id, dateTime,
                                                  criterionAndGrades);
            });
            Assert.AreEqual(exceptrionString, exception.Message);
        }

        [Test]
        public void SaveDiagnosticWithIncorrectGrade()
        {
            // arrange
            DateTime dateTime = DateTime.Now;
            Random randomizer = new Random();
            var keys = _diagnosticManager.GetCriterions().ToList();
            IDictionary<string, string> criterionAndGrades
                = new Dictionary<string, string>();
            var exceptrionString = "Not valid grade.";

            foreach (var key in keys)
            {
                var grade = randomizer.Next(5, 10).ToString();
                var pair = new KeyValuePair<string, string>(key, grade);
                criterionAndGrades.Add(pair);
            }

            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _diagnosticManager.SaveDiagnostic(_trip.Id, dateTime,
                                                  criterionAndGrades);
            });
            Assert.AreEqual(exceptrionString, exception.Message);
        }

        [Test]
        public void SaveDiagnosticWithIncorrectDefinitions()
        {
            // arrange
            DateTime dateTime = DateTime.Now;
            Random randomizer = new Random();
            var keys = _diagnosticManager.GetCriterions().ToList();
            keys[0] = "Неизвестный критерий.";
            IDictionary<string, string> criterionAndGrades
                = new Dictionary<string, string>();
            var exceptrionString = "Not valid definition.";

            foreach (var key in keys)
            {
                var grade = randomizer.Next(1, 5).ToString();
                var pair = new KeyValuePair<string, string>(key, grade);
                criterionAndGrades.Add(pair);
            }

            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _diagnosticManager.SaveDiagnostic(_trip.Id, dateTime,
                                                  criterionAndGrades);
            });
            Assert.AreEqual(exceptrionString, exception.Message);
        }

        [Test]
        public void GettingAllDiagnostics()
        {
            // arrange
            DateTime dateTime = DateTime.Now;
            var criterionAndGrades = GetFullSetOfCriterionsAndGrades();
            

            _diagnosticManager.SaveDiagnostic(_trip.Id, dateTime,
                                              criterionAndGrades);
            _diagnosticManager.SaveDiagnostic(_trip.Id, dateTime.AddDays(1),
                                              criterionAndGrades);
            _diagnosticManager.SaveDiagnostic(_trip.Id, dateTime.AddDays(2),
                                              criterionAndGrades);

            // act
            var diagnostics = _diagnosticManager.GetDiagnostics(_trip.Id);

            // assert
            Assert.AreEqual(3, diagnostics.Count());

        }

        [Test]
        public void DeleteDiagnostic()
        {
            // arrange
            var amountOfCriterions = _diagnosticManager.GetNumberOfCriterions();
            DateTime dateTime = DateTime.Now;
            var criterionsAndGrades = GetFullSetOfCriterionsAndGrades();
            var diagnosticId = _diagnosticManager.SaveDiagnostic(_trip.Id,
                                                                 dateTime,
                                                                 criterionsAndGrades);


            // act
            _diagnosticManager.DeleteDiagnostic(diagnosticId);
            var deletedDiagnostic =
                    _diagnosticManager.GetDiagnostic(diagnosticId);

            // assert
            Assert.AreEqual(null, deletedDiagnostic);
            Assert.AreEqual(amountOfCriterions,
                            _diagnosticManager.GetCriterions().Count());
        }

        [Test]
        public void UpdateDiagnostic()
        {
            // arrange
            DateTime dateTime = DateTime.Now;
            var criterionsAndGrades = GetFullSetOfCriterionsAndGrades();
            var diagnosticId = _diagnosticManager.SaveDiagnostic(_trip.Id,
                                                                 dateTime,
                                                                 criterionsAndGrades);
            var diagnostic = _diagnosticManager.GetDiagnostic(diagnosticId);

            var newDate = dateTime.AddDays(1);
            var newGrade = "0000";
            var criterionToChangeGrade = Resources.
                    Repositories.
                    CriterionDefinitionStrings.
                    Criterion18;

            // act
            criterionsAndGrades =
                _diagnosticManager.GetCriterionsAndGrades(diagnostic.Id);
            criterionsAndGrades[criterionToChangeGrade] = newGrade;
            
            _diagnosticManager.UpdateDiagnostic(diagnosticId,
                                                newDate,
                                                criterionsAndGrades);
            diagnostic = _diagnosticManager.GetDiagnostic(diagnosticId);
            var newCriterionsAndGrades =
                    _diagnosticManager.GetCriterionsAndGrades(diagnosticId);

            // assert
            Assert.AreEqual(newDate, diagnostic.Date);
            Assert.AreEqual(criterionsAndGrades, newCriterionsAndGrades);
        }
    }
}
