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
            var activityCards = _manager.Cards.GetActivityCards().
                                         Select(x => x.Id).
                                         ToList();

            var goalCards = _manager.Cards.GetMotivationGoalCards().
                                     Select(x => x.Id).
                                     ToList();

            var ids = activityCards.Concat(goalCards).ToList();

            // act
            var scheduleId = _manager.SaveSchedule(_user.Id, ids);
            var addedSchedule = _manager.GetSchedule(scheduleId);

            // assert
            Assert.AreEqual(ids.Count, addedSchedule.ScheduleItems.Count);
            Assert.AreEqual(1, addedSchedule.ScheduleItems[0].CardId);
            Assert.AreEqual(2, addedSchedule.ScheduleItems[1].CardId);
            Assert.AreEqual(3, addedSchedule.ScheduleItems[2].CardId);
            Assert.AreEqual(4, addedSchedule.ScheduleItems[3].CardId);
            Assert.AreEqual(_user.Id, addedSchedule.UserId);
        }

        [Test]
        public void CreateScheduleWithNullListOfIds()
        {
            // arrange
            var exceptionString = "Card ids are not set.";

            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _manager.SaveSchedule(_user.Id, null);
            });
            Assert.AreEqual(exceptionString, exception.Message);
        }

        [Test]
        public void CreateScheduleWithNotExistingCards()
        {
            // arrange
            var activityCards = _manager.Cards.GetActivityCards().
                                         Select(x => x.Id).
                                         ToList();

            var goalCards = _manager.Cards.GetMotivationGoalCards().
                                     Select(x => x.Id).
                                     ToList();

            var ids = activityCards.Concat(goalCards).ToList();
            ids.Add(33);
            var exceptionString = "Card with id: \"33\" doesn't exist.";

            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _manager.SaveSchedule(_user.Id, ids);
            });
            Assert.AreEqual(exceptionString, exception.Message);
        }

        [Test]
        public void CreateScheduleWithoutActivityCards()
        {
            // arrange
            var goalCards = _manager.Cards.GetMotivationGoalCards().
                                     Select(x => x.Id).
                                     ToList();
            var exceptionString = "Activity card is not set " +
                                  "in timetable. At least one " +
                                  "activity card must be set.";
            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _manager.SaveSchedule(_user.Id, goalCards);
            });
            Assert.AreEqual(exceptionString, exception.Message);
        }

        [Test]
        public void CreateScheduleWithoutMotivationGoalCards()
        {
            // arrange
            var activityCards = _manager.Cards.GetActivityCards().
                                         Select(x => x.Id).
                                         ToList();
            var exceptionString = "Motivation goal card is not set" +
                                  "in timetable. You need to set " +
                                  "one motivation goal card.";

            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _manager.SaveSchedule(_user.Id, activityCards);
            });
            Assert.AreEqual(exceptionString, exception.Message);
        }

        [Test]
        public void CreateScheduleWithEmptyListOfIds()
        {
            // arrange
            var exceptionString = "Card ids are not set.";

            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _manager.SaveSchedule(_user.Id, new List<int>());
            });
            Assert.AreEqual(exceptionString, exception.Message);
        }

        [Test]
        public void CreateScheduleWithMultipleMotivationGoalCards()
        {
            // arrange
            var goalType = _manager.Cards.CardTypes.GetMotivationGoalCardType();
            Card card = new Card()
            {
                PhotoPath = "path/to/photo5.jpg",
                CardTypeId = goalType.Id
            };
            _manager.Cards.SaveCard(card);

            var activityCards = _manager.Cards.GetActivityCards().
                                         Select(x => x.Id).
                                         ToList();

            var goalCards = _manager.Cards.GetMotivationGoalCards().
                                     Select(x => x.Id).
                                     ToList();

            var ids = activityCards.Concat(goalCards).ToList();

            var exceptionString = "Multiple motivation goal " +
                                  "cards are not allowed.";

            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _manager.SaveSchedule(_user.Id, ids);
            });
            Assert.AreEqual(exceptionString, exception.Message);
        }

        [Test]
        public void GetAllSchedules()
        {
            // arrange
            var activityCards = _manager.Cards.GetActivityCards().
                                         Select(x => x.Id).
                                         ToList();

            var goalCards = _manager.Cards.GetMotivationGoalCards().
                                     Select(x => x.Id).
                                     ToList();

            var ids = activityCards.Concat(goalCards).ToList();
            _manager.SaveSchedule(_user.Id, ids);
            _manager.SaveSchedule(_user.Id, ids);
            _manager.SaveSchedule(_user.Id, ids);

            // act
            var schedules = _manager.GetSchedules(_user.Id).ToList();
            
            // assert
            Assert.AreEqual(3, schedules.Count);
        }

        [Test]
        public void UpdateScheduleWithDeletingCards()
        {
            // arrange
            var activityCards = _manager.Cards.GetActivityCards().
                                         Select(x => x.Id).
                                         ToList();

            var goalCards = _manager.Cards.GetMotivationGoalCards().
                                     Select(x => x.Id).
                                     ToList();

            var ids = activityCards.Concat(goalCards).ToList();
            var scheduleId = _manager.SaveSchedule(_user.Id, ids);
            var addedSchedule = _manager.GetSchedule(scheduleId);

            // act
            var newCardIds = _manager.GetCardIds(scheduleId).ToList();
            newCardIds.RemoveAt(2);
            _manager.UpdateSchedule(scheduleId, newCardIds);
            addedSchedule = _manager.GetSchedule(scheduleId);

            // assert
            Assert.AreEqual(3, addedSchedule.ScheduleItems.Count);
            Assert.AreEqual(activityCards[0], addedSchedule.ScheduleItems[0].CardId);
            Assert.AreEqual(activityCards[1], addedSchedule.ScheduleItems[1].CardId);
            Assert.AreEqual(goalCards[0], addedSchedule.ScheduleItems[2].CardId);
        }

        [Test]
        public void UpdateScheduleWithAddingCards()
        {
            // arrange
            var activityType = _manager.Cards.CardTypes.GetActivityCardType();
            Card card = new Card()
            {
                PhotoPath = "path/to/photo5.jpg",
                CardTypeId = activityType.Id
            };

            var activityCards = _manager.Cards.GetActivityCards().
                                         Select(x => x.Id).
                                         ToList();

            var goalCards = _manager.Cards.GetMotivationGoalCards().
                                     Select(x => x.Id).
                                     ToList();

            var ids = activityCards.Concat(goalCards).ToList();
            var scheduleId = _manager.SaveSchedule(_user.Id, ids);
            var addedSchedule = _manager.GetSchedule(scheduleId);

            // act
            var newCardId = _manager.Cards.SaveCard(card);

            activityCards = _manager.Cards.GetActivityCards().
                                     Select(x => x.Id).
                                     ToList();
            ids = activityCards.Concat(goalCards).ToList();

            _manager.UpdateSchedule(scheduleId, ids);
            addedSchedule = _manager.GetSchedule(scheduleId);

            // assert
            Assert.AreEqual(5, addedSchedule.ScheduleItems.Count);
            Assert.AreEqual(activityCards[0], addedSchedule.ScheduleItems[0].CardId);
            Assert.AreEqual(activityCards[1], addedSchedule.ScheduleItems[1].CardId);
            Assert.AreEqual(activityCards[2], addedSchedule.ScheduleItems[2].CardId);
            Assert.AreEqual(activityCards[3], addedSchedule.ScheduleItems[3].CardId);
            Assert.AreEqual(goalCards[0], addedSchedule.ScheduleItems[4].CardId);
        }

        [Test]
        public void ComplexUpdateSchedule()
        {
            // arrange
            var activityType = _manager.Cards.CardTypes.GetActivityCardType();
            Card card = new Card()
            {
                PhotoPath = "path/to/photo5.jpg",
                CardTypeId = activityType.Id
            };

            var activityCards = _manager.Cards.GetActivityCards().
                                         Select(x => x.Id).
                                         ToList();

            var goalCards = _manager.Cards.GetMotivationGoalCards().
                                     Select(x => x.Id).
                                     ToList();

            var ids = activityCards.Concat(goalCards).ToList();
            var scheduleId = _manager.SaveSchedule(_user.Id, ids);
            var addedSchedule = _manager.GetSchedule(scheduleId);

            // act
            var newCardId = _manager.Cards.SaveCard(card);
            ids = _manager.GetCardIds(scheduleId).ToList();
            ids.Insert(1, newCardId);
            ids.RemoveAt(3);
            _manager.UpdateSchedule(scheduleId, ids);
            addedSchedule = _manager.GetSchedule(scheduleId);

            // assert
            Assert.AreEqual(4, addedSchedule.ScheduleItems.Count);
            Assert.AreEqual(1, addedSchedule.ScheduleItems[0].CardId);
            Assert.AreEqual(5, addedSchedule.ScheduleItems[1].CardId);
            Assert.AreEqual(2, addedSchedule.ScheduleItems[2].CardId);
            Assert.AreEqual(4, addedSchedule.ScheduleItems[3].CardId);
        }

        [Test]
        public void DeleteSchedule()
        {
            // arrange

            // act

            // assert
        }

        [Test]
        public void FinishSchedule()
        {
            
        }

        [Test]
        public void CompleteSchedule()
        {
            
        }
    }
}
