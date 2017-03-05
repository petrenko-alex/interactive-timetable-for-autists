using System.IO;
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
            Card card = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = 1
            };

            // act
            var cardId = _repository.SaveCard(card);

        }
    }
}
