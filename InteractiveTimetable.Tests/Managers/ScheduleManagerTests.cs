using System.IO;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Managers;
using InteractiveTimetable.BusinessLayer.Models;
using NUnit.Framework;
using SQLite;

namespace InteractiveTimetable.Tests.Managers
{
    [TestFixture]
    class ScheduleManagerTests
    {
        private SQLiteConnection _connection;
        private ScheduleManager _manager;
        private UserManager _userManager;
        private User _user;

        [SetUp]
        public void InitializationBeforeTests()
        {
            /* Creating connection */
            string dbPath = "TestsDatabase.db3";
            _connection = new SQLiteConnection(dbPath);

            /* Initializing managers */
            _manager = new ScheduleManager(_connection);
            _userManager = new UserManager(_connection);

            /* Creating user */
            _user = new User()
            {
                FirstName = "Alexander",
                LastName = "Petrenko",
                PatronymicName = "Andreevich",
                BirthDate = System.DateTime.Parse("25.07.1995").Date,
                PhotoPath = "avatar1.jpg",
            };
            _userManager.SaveUser(_user);

            /* Creating cards */
            var activityType = _manager.Cards.CardTypes.GetActivityCardType();
            var goalType = _manager.Cards.CardTypes.GetMotivationGoalCardType();

            Card card1 = new Card()
            {
                PhotoPath = "path/to/photo1.jpg",
                CardTypeId = activityType.Id
            };
            Card card2 = new Card()
            {
                PhotoPath = "path/to/photo2.jpg",
                CardTypeId = activityType.Id
            };
            Card card3 = new Card()
            {
                PhotoPath = "path/to/photo3.jpg",
                CardTypeId = activityType.Id
            };
            Card card4 = new Card()
            {
                PhotoPath = "path/to/photo4.jpg",
                CardTypeId = goalType.Id
            };
            _manager.Cards.SaveCard(card1);
            _manager.Cards.SaveCard(card2);
            _manager.Cards.SaveCard(card3);
            _manager.Cards.SaveCard(card4);
        }

        [TearDown]
        public void ShutDown()
        {
            _connection.Dispose();
            File.Delete("TestsDatabase.db3");
        }

        [Test]
        public void CreateSimpleSchedule()
        {
            // arrange
            var activityCards = _manager.Cards.GetActivityCards().ToList();
            var goalCards = _manager.Cards.GetMotivationGoalCards().ToList();


            // act

            // assert
        }
    }
}
