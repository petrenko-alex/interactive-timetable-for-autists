using System;
using System.IO;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.DataAccessLayer;
using NUnit.Framework;
using SQLite;

namespace InteractiveTimetable.Tests.Managers
{
    [TestFixture]
    class CardManagerTests
    {
        private SQLiteConnection _connection;
        private CardRepository _repository;

        public string GenerateRandomString(int stringLength)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            return new string(Enumerable.Repeat(chars, stringLength).
                                         Select(s => s[random.Next(s.Length)]).
                                         ToArray());
        }

        [SetUp]
        public void InitializationBeforeTests()
        {
            /* Creating connection */
            string dbPath = "TestsDatabase.db3";
            _connection = new SQLiteConnection(dbPath);

            /* Initialize repository */
            _repository = new CardRepository(_connection);
        }

        [TearDown]
        public void ShutDown()
        {
            _connection.Dispose();
            File.Delete("TestsDatabase.db3");
        }

        [Test]
        public void CreateSimpleCard()
        {
            // arrange
            CardType activityType = _repository.CardTypes.GetActivityCardType();
            Card card = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };

            // act
            var cardId = _repository.SaveCard(card);
            var addedCard = _repository.GetCard(cardId);

            // assert
            Assert.AreEqual(card, addedCard);
        }

        [Test]
        public void CreateCardWithIncorrectPath()
        {
            // arrange
            CardType activityType = _repository.CardTypes.GetActivityCardType();
            Card card = new Card()
            {
                PhotoPath = GenerateRandomString(1025),
                CardTypeId = activityType.Id
            };
            string exceptionString = "The length of the path " +
                                     "to the card's photo must be less " +
                                     "than 1024 symbols";

            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _repository.SaveCard(card);
            });
            Assert.AreEqual(exceptionString, exception.Message);
        }

        [Test]
        public void CreateCardWithScheduleItems()
        {
            // TODO: Implement when ScheduleItemRepository is done
        }
    }
}
