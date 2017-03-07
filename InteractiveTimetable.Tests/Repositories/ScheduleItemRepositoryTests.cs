using System;
using System.IO;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.DataAccessLayer;
using NUnit.Framework;
using SQLite;

namespace InteractiveTimetable.Tests.Repositories
{
    [TestFixture]
    class ScheduleItemRepositoryTests
    {
        private SQLiteConnection _connection;
        private ScheduleItemRepository _repository;

        [SetUp]
        public void InitializationBeforeTests()
        {
            /* Creating connection */
            string dbPath = "TestsDatabase.db3";
            _connection = new SQLiteConnection(dbPath);

            /* Initialize repository */
            _repository = new ScheduleItemRepository(_connection);
        }

        [TearDown]
        public void ShutDown()
        {
            _connection.Dispose();
            File.Delete("TestsDatabase.db3");
        }

        [Test]
        public void SaveScheduleItem()
        {
            // arrange
            ScheduleItem item = new ScheduleItem()
            {
                CardId = 1,
                OrderNumber = 1,
                ScheduleId = 1
            };
            

            // act
            var itemId = _repository.SaveScheduleItem(item);
            var addedItem = _repository.GetScheduleItem(itemId);

            // assert
            Assert.AreEqual(item, addedItem);
        }

        [Test]
        public void SaveWithNullItem()
        {
            // arrange
            ScheduleItem item = null;
            string exceptionString = "Schedule item is not set";

            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _repository.SaveScheduleItem(item);
            });
            Assert.AreEqual(exceptionString, exception.Message);
        }

        [Test]
        public void SaveWithIncorrectOrderNumber()
        {
            // arrange
            ScheduleItem item = new ScheduleItem()
            {
                CardId = 1,
                OrderNumber = 0,
                ScheduleId = 1
            };
            string exceptionString = "Order number can't be less " +
                                     "or equal to 0";

            // act
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _repository.SaveScheduleItem(item);
            });
            Assert.AreEqual(exceptionString, exception.Message);
        }

        [Test]
        public void GetAllScheduleItems()
        {
            // arrange
            ScheduleItem item1 = new ScheduleItem()
            {
                CardId = 1,
                OrderNumber = 1,
                ScheduleId = 1
            };

            ScheduleItem item2 = new ScheduleItem()
            {
                CardId = 1,
                OrderNumber = 2,
                ScheduleId = 1
            };
            _repository.SaveScheduleItem(item1);
            _repository.SaveScheduleItem(item2);

            // act
            var items = _repository.GetScheduleItems().ToList();

            // assert
            Assert.AreEqual(2, items.Count);
        }

        [Test]
        public void GetScheduleItemsBySchedule()
        {
            // arrange
            ScheduleItem item1 = new ScheduleItem()
            {
                CardId = 1,
                OrderNumber = 1,
                ScheduleId = 1
            };

            ScheduleItem item2 = new ScheduleItem()
            {
                CardId = 1,
                OrderNumber = 2,
                ScheduleId = 1
            };

            ScheduleItem item3 = new ScheduleItem()
            {
                CardId = 1,
                OrderNumber = 2,
                ScheduleId = 2
            };
            _repository.SaveScheduleItem(item1);
            _repository.SaveScheduleItem(item2);
            _repository.SaveScheduleItem(item3);

            // act
            var items = _repository.GetScheduleItemsOfSchedule(1).ToList();

            // assert
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0].ScheduleId);
            Assert.AreEqual(1, items[1].ScheduleId);
        }

        [Test]
        public void EditScheduleItem()
        {
            // arrange
            ScheduleItem item = new ScheduleItem()
            {
                CardId = 1,
                OrderNumber = 1,
                ScheduleId = 1
            };
            var itemId = _repository.SaveScheduleItem(item);
            var addedItem = _repository.GetScheduleItem(itemId);

            // act
            addedItem.OrderNumber = 5;
            _repository.SaveScheduleItem(addedItem);
            addedItem = _repository.GetScheduleItem(itemId);

            // assert
            Assert.AreEqual(5, addedItem.OrderNumber);
        }

        [Test]
        public void DeleteScheduleItem()
        {
            // arrange
            ScheduleItem item = new ScheduleItem()
            {
                CardId = 1,
                OrderNumber = 1,
                ScheduleId = 1
            };
            var itemId = _repository.SaveScheduleItem(item);
            _repository.GetScheduleItem(itemId);

            // act
            _repository.DeleteScheduleItem(itemId);
            var deletedItem = _repository.GetScheduleItem(itemId);

            // assert
            Assert.AreEqual(null, deletedItem);

        }

        [Test]
        public void DeleteNotExistingScheduleItem()
        {
            // arrange
            int notExistingId = 22;

            // act
            _repository.DeleteScheduleItem(notExistingId);
            var deletedItem = _repository.GetScheduleItem(notExistingId);

            // asert
            Assert.AreEqual(null, deletedItem);
            Assert.Pass("No exceptions");
        }
    }
}
