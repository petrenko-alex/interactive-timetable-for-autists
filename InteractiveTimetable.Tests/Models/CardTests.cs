using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveTimetable.BusinessLayer.Contracts;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.DataAccessLayer;
using NUnit.Framework;
using SQLite;

namespace InteractiveTimetable.Tests.Models
{
    [TestFixture]
    class CardTests
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
        public void TestGetHashCode()
        {
            // arrange
            CardType activityType = _repository.CardTypes.GetActivityCardType();
            Card card1 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };

            Card card2 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };

            // act
            var hash1 = card1.GetHashCode();
            var hash2 = card2.GetHashCode();
            var hash3 = card1.GetHashCode();

            // assert
            /* Hash equals for equal objects */
            Assert.AreEqual(true, hash1 == hash2);

            /* Hash equals for one object */
            Assert.AreEqual(true, hash1 == hash3);
        }

        [Test]
        public void TestEquals()
        {
            // arrange
            CardType activityType = _repository.CardTypes.GetActivityCardType();
            Card card1 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };

            Card card2 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };
            BusinessEntityBase card3 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };
            var str = "string";

            // act
            bool isSameObjectsEqual = card1.Equals(card2);
            bool isObjectEqualToItself = card1.Equals(card1);
            bool isObjectEqualToAnotherWithCasting = card1.Equals(card3);
            bool isObjectNotEqualToNull = card1.Equals(null);
            bool isObjectNotEqualToDifferentTypeObject = card1.Equals(str);

            // assert
            Assert.IsTrue(isSameObjectsEqual);
            Assert.IsTrue(isObjectEqualToItself);
            Assert.IsTrue(isObjectEqualToAnotherWithCasting);
            Assert.IsFalse(isObjectNotEqualToNull);
            Assert.IsFalse(isObjectNotEqualToDifferentTypeObject);
        }
    }
}
