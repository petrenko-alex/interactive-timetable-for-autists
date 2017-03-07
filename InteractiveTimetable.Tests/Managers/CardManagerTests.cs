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
    class CardManagerTests
    {
        private SQLiteConnection _connection;
        private CardManager _manager;

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
            _manager = new CardManager(_connection);
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
            CardType activityType = _manager.CardTypes.GetActivityCardType();
            Card card = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };

            // act
            var cardId = _manager.SaveCard(card);
            var addedCard = _manager.GetCard(cardId);

            // assert
            Assert.AreEqual(card, addedCard);
        }

        [Test]
        public void CreateCardWithIncorrectPath()
        {
            // arrange
            CardType activityType = _manager.CardTypes.GetActivityCardType();
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
                _manager.SaveCard(card);
            });
            Assert.AreEqual(exceptionString, exception.Message);
        }

        [Test]
        public void CreateCardWithScheduleItems()
        {
            // arrange
            CardType activityType = _manager.CardTypes.GetActivityCardType();
            ScheduleItem item1 = new ScheduleItem()
            {
                OrderNumber = 1
            };
            ScheduleItem item2 = new ScheduleItem()
            {
                OrderNumber = 2
            };
            Card card = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id,
                ScheduleItems = new List<ScheduleItem>()
                {
                    item1,
                    item2
                }
            };

            // act
            var cardId = _manager.SaveCard(card);
            var addedCard = _manager.GetCard(cardId);

            // assert
            Assert.AreEqual(2, addedCard.ScheduleItems.Count);
            Assert.AreEqual(addedCard.Id, addedCard.ScheduleItems[0].CardId);
            Assert.AreEqual(addedCard.Id, addedCard.ScheduleItems[1].CardId);
        }

        [Test]
        public void UpdateSimpleCard()
        {
            // arrange
            CardType activityType = _manager.CardTypes.GetActivityCardType();
            Card card = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };
            var cardId = _manager.SaveCard(card);
            var addedCard = _manager.GetCard(cardId);
            var newPath = "path/to/song.wav";

            // act
            addedCard.PhotoPath = newPath;
            _manager.SaveCard(addedCard);
            addedCard = _manager.GetCard(cardId);

            // assert
            Assert.AreEqual(newPath, addedCard.PhotoPath);
            Assert.AreEqual(card.CardTypeId, addedCard.CardTypeId);
        }

        [Test]
        public void UpdateCardWithScheduleItems()
        {
            // TODO: Implement this when scheduleItem repository is ready
            // use UserManagerTests->EditUserWithTripEditing method as hint
//            // arrange
//            CardType activityType = _manager.CardTypes.GetActivityCardType();
//            ScheduleItem item1 = new ScheduleItem()
//            {
//                OrderNumber = 1
//            };
//            ScheduleItem item2 = new ScheduleItem()
//            {
//                OrderNumber = 2
//            };
//            ScheduleItem item3 = new ScheduleItem()
//            {
//                OrderNumber = 3
//            };
//            Card card = new Card()
//            {
//                PhotoPath = "path/to/photo.jpg",
//                CardTypeId = activityType.Id,
//                ScheduleItems = new List<ScheduleItem>()
//                {
//                    item1,
//                    item2
//                }
//            };
//            var cardId = _manager.SaveCard(card);
//            var addedCard = _manager.GetCard(cardId);
//
//            // act
//            addedCard.ScheduleItems[1].OrderNumber = 5;
//            addedCard.ScheduleItems.Add(item3);
//            addedCard.ScheduleItems.RemoveAt(0);
//            _manager.SaveCard(addedCard);
//            addedCard = _manager.GetCard(cardId);
//
//            // assert
//            Assert.AreEqual(2, addedCard.ScheduleItems.Count);
//            Assert.AreEqual(5, addedCard.ScheduleItems[0].OrderNumber);
//            Assert.AreEqual(3, addedCard.ScheduleItems[1].OrderNumber);
            Assert.Pass("Need to implement");
        }

        [Test]
        public void DeleteSimpleCard()
        {
            // arrange
            CardType activityType = _manager.CardTypes.GetActivityCardType();
            Card card = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };
            var cardId = _manager.SaveCard(card);

            // act
            _manager.DeleteCard(cardId);
            var deletedCard = _manager.GetCard(cardId);

            // assert
            Assert.AreEqual(null, deletedCard);
        }

        [Test]
        public void DeleteWithScheduleItems()
        {
            // arrange
            CardType activityType = _manager.CardTypes.GetActivityCardType();
            ScheduleItem item1 = new ScheduleItem()
            {
                OrderNumber = 1
            };
            ScheduleItem item2 = new ScheduleItem()
            {
                OrderNumber = 2
            };
            Card card = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id,
                ScheduleItems = new List<ScheduleItem>()
                {
                    item1,
                    item2
                }
            };
            var cardId = _manager.SaveCard(card);
            card = _manager.GetCard(cardId);
            var item1Id = card.ScheduleItems[0].Id;
            var item2Id = card.ScheduleItems[1].Id;

            // act
            _manager.DeleteCard(cardId);
            var deletedCard = _manager.GetCard(cardId);
            // TODO: remove comment sign when ScheduleItemRepository is ready
            //var deletedItem1 = _scheduleItemRepository.GetScheduleItem(item1Id);
            //var deletedItem2 = _scheduleItemRepository.GetScheduleItem(item2Id);

            // assert
            Assert.AreEqual(null, deletedCard);
            //Assert.AreEqual(null, deletedItem1);
            //Assert.AreEqual(null, deletedItem2);
        }

        [Test]
        public void GetActivityCards()
        {
            // arrange
            CardType activityType = 
                _manager.CardTypes.GetActivityCardType();
            CardType motivationGoalType =
                    _manager.CardTypes.GetMotivationGoalCardType();
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
            Card card3 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = motivationGoalType.Id
            };
            _manager.SaveCard(card1);
            _manager.SaveCard(card2);
            _manager.SaveCard(card3);

            // act
            var cards = _manager.GetActivityCards().ToList();

            // assert
            Assert.AreEqual(2, cards.Count);
        }

        [Test]
        public void GetMotivationGoalCards()
        {
            // arrange
            CardType activityType =
                _manager.CardTypes.GetActivityCardType();
            CardType motivationGoalType =
                    _manager.CardTypes.GetMotivationGoalCardType();
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
            Card card3 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = motivationGoalType.Id
            };
            _manager.SaveCard(card1);
            _manager.SaveCard(card2);
            _manager.SaveCard(card3);

            // act
            var cards = _manager.GetMotivationGoalCards().ToList();

            // assert
            Assert.AreEqual(1, cards.Count);
        }

        [Test]
        public void IsActivityCard()
        {
            // arrange
            CardType activityType =
                _manager.CardTypes.GetActivityCardType();
            CardType motivationGoalType =
                    _manager.CardTypes.GetMotivationGoalCardType();
            Card card1 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };
            Card card2 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = motivationGoalType.Id
            };
            var card1Id = _manager.SaveCard(card1);
            var card2Id = _manager.SaveCard(card2);

            // act
            var isActivity = _manager.IsActivityCard(card1Id);
            var isNotActivity = _manager.IsActivityCard(card2Id);

            // assert
            Assert.AreEqual(true, isActivity);
            Assert.AreEqual(false, isNotActivity);
        }

        [Test]
        public void IsMotivationGoalCard()
        {
            // arrange
            CardType activityType =
                _manager.CardTypes.GetActivityCardType();
            CardType motivationGoalType =
                    _manager.CardTypes.GetMotivationGoalCardType();
            Card card1 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };
            Card card2 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = motivationGoalType.Id
            };
            var card1Id = _manager.SaveCard(card1);
            var card2Id = _manager.SaveCard(card2);

            // act
            var isMotivationGoalCard = _manager.IsMotivationGoalCard(card2Id);
            var isNotMotivationGoalCard = _manager.IsMotivationGoalCard(card1Id);

            // assert
            Assert.AreEqual(true, isMotivationGoalCard);
            Assert.AreEqual(false, isNotMotivationGoalCard);

        }

        [Test]
        public void IsCardInPresentTimetable()
        {
            // arrange
            CardType activityType = _manager.CardTypes.GetActivityCardType();
            ScheduleItem item1 = new ScheduleItem()
            {
                OrderNumber = 1
            };
            ScheduleItem item2 = new ScheduleItem()
            {
                OrderNumber = 2
            };
            Card card1 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id,
                ScheduleItems = new List<ScheduleItem>()
                {
                    item1,
                    item2
                }
            };

            Card card2 = new Card()
            {
                PhotoPath = "path/to/photo1.jpg",
                CardTypeId = activityType.Id,
            };
            var card1Id = _manager.SaveCard(card1);
            var card2Id = _manager.SaveCard(card2);

            // act
            var isPresent = _manager.IsCardInPresentTimetable(card1Id);
            var notPresent = _manager.IsCardInPresentTimetable(card2Id);

            // assert
            Assert.AreEqual(true, isPresent);
            Assert.AreEqual(false, notPresent);
        }

        [Test]
        public void IsCardExist()
        {
            // arrange
            int notExistCardId = 22;
            CardType activityType = _manager.CardTypes.GetActivityCardType();
            Card card = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };
            var cardId = _manager.SaveCard(card);

            // act
            var isExist = _manager.IsCardExist(cardId);
            var notExist = _manager.IsCardExist(notExistCardId);

            // assert
            Assert.AreEqual(true, isExist);
            Assert.AreEqual(false, notExist);
        }
    }
}
